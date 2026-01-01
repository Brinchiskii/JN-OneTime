<script setup lang="ts">
import Timesheet from '../components/Timesheet.vue'
import { onMounted, ref, computed } from 'vue'
import { useTimesheetStore } from '../stores/timesheetStore'
import type { TimesheetRow } from '@/types'
import ManagerTeamCard from '@/components/ManagerTeamCard.vue'
import DatePicker from '@/components/DatePicker.vue'
import Sidebar from '@/components/Sidebar.vue'

const timesheetStore = useTimesheetStore()
const isLoading = ref(true)

const loading = () => {
  isLoading.value = true
  timesheetStore.loadTeamRows()
    .finally(() => {
      isLoading.value = false
    })
}

const viewMode = ref<'pending' | 'all'>('pending')

const filteredAndSortedRows = computed(() => {
  let entries = Object.entries(timesheetStore.teamRows)

  if (viewMode.value === 'pending') {
    entries = entries.filter(([_, rows]) => {
      return rows[0]?.status === 0
    })
  }

  return entries.sort((a, b) => {
    const [nameA, sheetsA] = a
    const [nameB, sheetsB] = b

    const statusA = sheetsA[0]?.status
    const statusB = sheetsB[0]?.status

    // Hjælpefunktion: Tildel "Vigtighed" (Lavt tal = Kommer først)
    const getRank = (s: number | null | undefined): number => {
      // 1. Pending (Afventer)
      if (s === 0) return 1
      // 2. Kladde
      if (s === 3) return 2
      // 3. Ikke oprettet (null eller undefined)
      if (s === null || s === undefined || s === -1) return 3
      // 4. Afvist
      if (s === 2) return 4
      // 5. Godkendt (Mindst vigtig)
      if (s === 1) return 5

      return 99
    }
    const rankA = getRank(statusA)
    const rankB = getRank(statusB)
    if (rankA !== rankB) {
      return rankA - rankB
    }
    return nameA.localeCompare(nameB)
  })
})

const pendingCount = computed(() => {
  return Object.values(timesheetStore.teamRows)
    .filter(rows => rows[0]?.status === 0)
    .length
})
const statusCounts = computed(() => {
  const allSheets = Object.values(timesheetStore.teamRows)

  return {
    notSubmitted: allSheets.filter(rows => !rows[0] || rows[0].status === null || rows[0].status === 3 || rows[0].status === -1).length,

    rejected: allSheets.filter(rows => rows[0]?.status === 2).length,

    approved: allSheets.filter(rows => rows[0]?.status === 1).length
  }
})

onMounted(() => {
  loading()
})

</script>

<template>
  <Sidebar></Sidebar>
  <div class="flex-grow-1 p-4 p-lg-5 overflow-auto">
    <h3>Team Oversigt</h3>
    <h5 class="my-1">{{ timesheetStore.weekHeader }}</h5>
    <div class="d-flex align-items-center mb-1">
      <DatePicker @change="loading" @click="loading"></DatePicker>
      <div class="btn-group ms-2" role="group" aria-label="View Mode Toggle">
        <button type="button" class="btn" :class="viewMode === 'pending' ? 'btn-dark' : 'btn-outline-dark'"
          @click="viewMode = 'pending'">
          Afventer
          <span v-if="pendingCount > 0" class="badge bg-white text-dark ms-1">
            {{ pendingCount }}
          </span>
        </button>

        <button type="button" class="btn" :class="viewMode === 'all' ? 'btn-dark' : 'btn-outline-dark'"
          @click="viewMode = 'all'">
          Alle
        </button>
      </div>
    </div>

    <span class="spinner-border spinner-border-sm" v-if="isLoading"></span>
    <span class="ms-2" v-if="isLoading">Loading...</span>

    <div v-else-if="viewMode === 'pending' && filteredAndSortedRows.length === 0" class="text-center py-5 bg-light rounded">

      <div v-if="statusCounts.notSubmitted > 0 || statusCounts.rejected > 0">

        <i class="bi bi-clipboard-check display-4 text-muted mb-2"></i>
        <h5>Ingen nye til godkendelse</h5>
        <p class="text-muted mb-4">Men der er stadig uafsluttede ugeskemaer i teamet:</p>

        <div class="d-flex justify-content-center gap-4 flex-wrap">

          <div v-if="statusCounts.notSubmitted > 0" class="card border-0 shadow-sm p-3" style="min-width: 200px;">
            <div class="text-warning mb-1">
              <i class="bi bi-hourglass-split fs-2"></i>
            </div>
            <h3 class="fw-bold mb-0">{{ statusCounts.notSubmitted }}</h3>
            <small class="text-muted">Mangler at indsende</small>
          </div>

          <div v-if="statusCounts.rejected > 0" class="card border-0 shadow-sm p-3" style="min-width: 200px;">
            <div class="text-danger mb-1">
              <i class="bi bi-exclamation-circle fs-2"></i>
            </div>
            <h3 class="fw-bold mb-0">{{ statusCounts.rejected }}</h3>
            <small class="text-muted">Er blevet afvist</small>
          </div>

        </div>
      </div>

      <div v-else>
        <i class="bi bi-check-circle-fill display-4 text-success mb-2"></i>
        <h5>Alt er klaret!</h5>
        <p class="text-muted mb-0">Alle ugeskemaer for denne uge er godkendt.</p>
      </div>

      <button class="btn btn-outline-dark mt-4" @click="viewMode = 'all'">
        Vis alle medarbejdere
      </button>

    </div>

    <ManagerTeamCard v-else v-for="[userName, rows] in filteredAndSortedRows" :key="userName" :userName="userName"
      :rows="rows[0]!" :status="rows[0]!.status" @refresh="loading">
    </ManagerTeamCard>
  </div>

</template>

<style scoped></style>