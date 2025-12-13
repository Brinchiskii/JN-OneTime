<script setup lang="ts">
import { useAuditLogsStore } from '@/stores/AuditlogsStore';
import { onMounted, computed, ref } from 'vue';
import { type Log } from '@/types'

const AuditlogsStore = useAuditLogsStore()

onMounted(async() => {
    await AuditlogsStore.getLogs()
})

const startDate = ref()

const changeStartDate = (e: any) => {
    startDate.value = e.target.value
}

const endDate = ref()

const changeEndDate = (e: any) => {
    endDate.value = e.target.value
}


const filteredLogs = computed(() => {
    let data: Log[] = AuditlogsStore.logs
    
    if(startDate.value){
        const start = new Date(startDate.value)
        data = data.filter(log => new Date(log.timestamp) >= start)
    }
    
    if(endDate.value){
        const end = new Date(endDate.value)
        end.setHours(23, 59, 99, 999)
    
        data = data.filter(log => new Date(log.timestamp) <= end)
    }

    return data
})

</script>
<template>
  <div class="flex-grow-1 p-4 p-lg-5 overflow-auto card">
    

        <div
        class="card-header btn btn-white bg-white border shadow-sm date-picker-wrapper d-flex align-items-center gap-2"
        >
        <i class="bi bi-calendar3"></i>
        <input type="date" class="date-input-hidden" @change="changeStartDate" />
        -
        <i class="bi bi-calendar3"></i>
        <input type="date" class="date-input-hidden" @change="changeEndDate" />
    </div>

    <div class="card-body">
        <table class="custom-table">
            <thead>
                <tr>
                    <th>auditLogId</th>
                    <th>timestamp</th>
                    <th>action</th>
                    <th>entityType</th>
                    <th>entityId</th>
                    <th>actorUserId</th>
                    <th>actorUserName</th>
                    <th>details</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="log in filteredLogs">
                    <td>{{ log.auditLogId }}</td>
                    <td>{{ log.timestamp }}</td>
                    <td>{{ log.action }}</td>
                    <td>{{ log.entityType }}</td>
                    <td>{{ log.entityId }}</td>
                    <td>{{ log.actorUserId }}</td>
                    <td>{{ log.actorUsername }}</td>
                    <td>{{ log.details }}</td>
                </tr>
                </tbody>
        </table>
    </div>
  </div>
</template>
