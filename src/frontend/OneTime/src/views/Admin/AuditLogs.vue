<script setup lang="ts">
import { useAuditLogsStore } from '@/stores/AuditlogsStore'
import { onMounted, computed, ref } from 'vue'
import type { Log } from '@/types' // Sørg for at Log typen er importeret

const auditLogsStore = useAuditLogsStore()
const loading = ref(false)

// --- STATE ---
const searchQuery = ref('')
const startDate = ref('')
const endDate = ref('')

// Sortering state
const sortColumn = ref<keyof Log | ''>('timestamp')
const sortDirection = ref<'asc' | 'desc'>('desc') // Nyeste først som standard

onMounted(async () => {
  loading.value = true
  await auditLogsStore.getLogs()
  loading.value = false
})

// --- HELPERS ---

// Formater dato pænt (DD-MM-YYYY HH:mm)
const formatDate = (dateString: string) => {
  if (!dateString) return '-'
  return new Date(dateString).toLocaleString('da-DK', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit'
  })
}

// Farvekoder til handlinger
const getActionBadge = (action: string) => {
  const act = action.toLowerCase()
  if (act.includes('create') || act.includes('add')) return { class: 'bg-success bg-opacity-10 text-success', icon: 'bi-plus-circle' }
  if (act.includes('update') || act.includes('edit')) return { class: 'bg-primary bg-opacity-10 text-primary', icon: 'bi-pencil' }
  if (act.includes('delete') || act.includes('remove')) return { class: 'bg-danger bg-opacity-10 text-danger', icon: 'bi-trash' }
  if (act.includes('login')) return { class: 'bg-info bg-opacity-10 text-info', icon: 'bi-box-arrow-in-right' }
  return { class: 'bg-secondary bg-opacity-10 text-secondary', icon: 'bi-activity' }
}

// Sorterings funktion
const sortTable = (column: keyof Log) => {
  if (sortColumn.value === column) {
    sortDirection.value = sortDirection.value === 'asc' ? 'desc' : 'asc'
  } else {
    sortColumn.value = column
    sortDirection.value = 'asc'
  }
}

// --- FILTERING LOGIC ---
const filteredLogs = computed(() => {
  let data = [...auditLogsStore.logs] // Lav en kopi for ikke at mutere store direkte

  // 1. Filtrer på dato
  if (startDate.value) {
    const start = new Date(startDate.value)
    start.setHours(0, 0, 0, 0)
    data = data.filter((log) => new Date(log.timestamp) >= start)
  }

  if (endDate.value) {
    const end = new Date(endDate.value)
    end.setHours(23, 59, 59, 999)
    data = data.filter((log) => new Date(log.timestamp) <= end)
  }

  // 2. Filtrer på søgning (ActorUserName + lidt ekstra for brugervenlighed)
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    data = data.filter(log => 
      (log.actorUserName && log.actorUserName.toLowerCase().includes(query)) ||
      (log.action && log.action.toLowerCase().includes(query)) ||
      (log.entityType && log.entityType.toLowerCase().includes(query))
    )
  }

  // 3. Sortering
  if (sortColumn.value) {
    data.sort((a, b) => {
      const valA = a[sortColumn.value as keyof Log]
      const valB = b[sortColumn.value as keyof Log]

      if (valA == null) return 1
      if (valB == null) return -1

      if (valA < valB) return sortDirection.value === 'asc' ? -1 : 1
      if (valA > valB) return sortDirection.value === 'asc' ? 1 : -1
      return 0
    })
  }

  return data
})
</script>

<template>
  <div class="flex-grow-1 p-4 p-lg-5 overflow-auto card h-100 border-0 shadow-none">
    
    <div class="d-flex justify-content-between align-items-center mb-5">
      <div>
        <h6 class="text-uppercase text-muted fw-bold mb-2" style="font-size: 0.75rem; letter-spacing: 1px">
          Sikkerhed & Sporing
        </h6>
        <h2 class="fw-bold mb-0 text-dark">Audit Logs</h2>
      </div>
      <button class="btn btn-light text-muted border" @click="auditLogsStore.getLogs()" :disabled="loading">
        <i class="bi bi-arrow-clockwise" :class="{'spin-icon': loading}"></i> Opdater
      </button>
    </div>

    <div class="admin-card p-0 mb-4">
      
      <div class="p-3 border-bottom d-flex flex-wrap gap-3 bg-light bg-opacity-50 align-items-center">
        
        <div class="position-relative" style="min-width: 300px;">
           <i class="bi bi-search position-absolute text-muted" style="left: 12px; top: 50%; transform: translateY(-50%);"></i>
           <input 
              type="text" 
              class="form-control ps-5 border-light shadow-sm" 
              placeholder="Søg efter bruger eller handling..." 
              v-model="searchQuery" 
           />
        </div>

        <div class="vr mx-2 d-none d-md-block text-muted opacity-25"></div>

        <div class="d-flex align-items-center gap-2 bg-white px-3 py-1 rounded border shadow-sm">
           <span class="text-muted small fw-bold text-uppercase" style="font-size: 0.7rem;">Periode:</span>
           <input type="date" class="border-0 text-muted small outline-none" v-model="startDate" style="outline: none;" />
           <i class="bi bi-arrow-right-short text-muted"></i>
           <input type="date" class="border-0 text-muted small outline-none" v-model="endDate" style="outline: none;" />
           
           <button v-if="startDate || endDate" class="btn btn-link btn-sm text-decoration-none p-0 ms-2 text-danger" 
                   @click="startDate = ''; endDate = ''" title="Nulstil datoer">
             <i class="bi bi-x-lg"></i>
           </button>
        </div>
        
        <div class="ms-auto text-muted small">
            Viser <strong>{{ filteredLogs.length }}</strong> logs
        </div>
      </div>

      <div class="table-responsive">
        <table class="table-admin mb-0">
          <thead>
            <tr>
              <th @click="sortTable('timestamp')" class="cursor-pointer user-select-none" style="width: 180px;">
                Tidspunkt
                <span v-if="sortColumn === 'timestamp'" class="text-primary">
                  <i :class="sortDirection === 'asc' ? 'bi bi-sort-numeric-down' : 'bi bi-sort-numeric-up-alt'"></i>
                </span>
                <span v-else class="text-muted opacity-25"><i class="bi bi-arrow-down-up"></i></span>
              </th>

              <th @click="sortTable('action')" class="cursor-pointer user-select-none">
                Handling
                <span v-if="sortColumn === 'action'" class="text-primary ms-1">
                    <i :class="sortDirection === 'asc' ? 'bi bi-sort-alpha-down' : 'bi bi-sort-alpha-up-alt'"></i>
                </span>
                 <span v-else class="text-muted opacity-25"><i class="bi bi-arrow-down-up"></i></span>
              </th>

              <th @click="sortTable('actorUserName')" class="cursor-pointer user-select-none">
                Udført af
                <span v-if="sortColumn === 'actorUserName'" class="text-primary ms-1">
                    <i :class="sortDirection === 'asc' ? 'bi bi-sort-alpha-down' : 'bi bi-sort-alpha-up-alt'"></i>
                </span>
                 <span v-else class="text-muted opacity-25"><i class="bi bi-arrow-down-up"></i></span>
              </th>

              <th @click="sortTable('entityType')" class="cursor-pointer user-select-none">
                Element
                <span v-if="sortColumn === 'entityType'" class="text-primary ms-1">
                    <i :class="sortDirection === 'asc' ? 'bi bi-sort-alpha-down' : 'bi bi-sort-alpha-up-alt'"></i>
                </span>
                 <span v-else class="text-muted opacity-25"><i class="bi bi-arrow-down-up"></i></span>
              </th>

              <th>Detaljer</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="loading">
                <td colspan="5" class="text-center py-5">
                    <div class="spinner-border text-primary" role="status"></div>
                    <div class="mt-2 text-muted">Indlæser logs...</div>
                </td>
            </tr>

            <tr v-else-if="filteredLogs.length === 0">
                <td colspan="5" class="text-center py-5 text-muted">
                    <i class="bi bi-search display-6 mb-3 d-block opacity-25"></i>
                    Ingen logs fundet i denne periode
                </td>
            </tr>

            <tr v-for="log in filteredLogs" :key="log.auditLogId">
              
              <td class="text-muted" style="font-family: monospace; font-size: 0.9rem;">
                  {{ formatDate(log.timestamp) }}
              </td>

              <td>
                <span class="badge rounded-pill fw-normal px-2 py-1 border"
                      :class="getActionBadge(log.action).class">
                  <i :class="['bi me-1', getActionBadge(log.action).icon]"></i>
                  {{ log.action }}
                </span>
              </td>

              <td>
                <div class="d-flex align-items-center gap-2">
                    <div class="avatar-xs bg-light rounded-circle d-flex align-items-center justify-content-center text-muted fw-bold border" 
                         style="width: 28px; height: 28px; font-size: 0.75rem;">
                         {{ log.actorUserName ? log.actorUserName.charAt(0).toUpperCase() : '?' }}
                    </div>
                    <div>
                        <div class="fw-medium text-dark">{{ log.actorUserName || 'System' }}</div>
                        <div class="small text-muted" style="font-size: 0.75rem;">ID: {{ log.actorUserId }}</div>
                    </div>
                </div>
              </td>

              <td>
                 <span class="text-dark fw-medium">{{ log.entityType }}</span>
                 <span class="text-muted ms-1 small">(#{{ log.entityId }})</span>
              </td>

              <td class="text-truncate" style="max-width: 300px;" :title="log.details">
                 <small class="text-muted">{{ log.details }}</small>
              </td>

            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* Genbruger stilen fra dine andre admin sider */

.admin-card {
  background: white;
  border: 1px solid rgba(0,0,0,0.08);
  border-radius: 12px;
  box-shadow: 0 2px 12px rgba(0,0,0,0.03);
  overflow: hidden;
}

.table-admin {
  width: 100%;
  border-collapse: collapse;
}

.table-admin th {
  background: #f8f9fa;
  color: #6c757d;
  font-weight: 600;
  font-size: 0.85rem;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  padding: 16px;
  border-bottom: 2px solid #e9ecef;
  white-space: nowrap;
}

.table-admin td {
  padding: 16px;
  vertical-align: middle;
  border-bottom: 1px solid #f1f3f5;
  font-size: 0.95rem;
  color: #495057;
}

.table-admin tbody tr:hover {
  background-color: #f8f9fa;
}

input[type="date"] {
    cursor: pointer;
}

.spin-icon {
    animation: spin 1s infinite linear;
}

@keyframes spin {
    from { transform: rotate(0deg); }
    to { transform: rotate(360deg); }
}
</style>