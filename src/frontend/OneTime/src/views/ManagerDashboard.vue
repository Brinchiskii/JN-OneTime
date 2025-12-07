<script setup lang="ts">
import Timesheet from '../components/Timesheet.vue'
import { onMounted } from 'vue'
import { useTimesheetStore } from '../stores/timesheetStore'

const timesheetStore = useTimesheetStore()

onMounted(() => {
  timesheetStore.loadTeamRows()
})

</script>

<template>
  <div class="flex-grow-1 p-4 p-lg-5 overflow-auto">
    <h5 class="my-1">Uge {{ timesheetStore.periodText }}</h5>
    <div class="manager-card" v-for="[userName, rows] in Object.entries(timesheetStore.teamRows)">
      <div class="card-header-custom">
        <div class="d-flex align-items-center gap-3">
          <div class="avatar">{{ userName.charAt(0) }}</div>
          <div>
            <div class="fw-bold fs-5">{{ userName }}</div>
            <div class="small text-muted">
              Total: <span class="fw-bold text-dark">{{ '38' }}t</span>
            </div>
          </div>
        </div>
        <button class="btn btn-success btn-sm px-3 rounded-pill" @click="">
          <i class="bi bi-check-lg me-1"></i> <span>Godkend</span>
        </button>
      </div>

      <Timesheet
        :rows="rows"
        :weekDays="timesheetStore.weekDays"
        :readonly="true"
      ></Timesheet>
    </div>
  </div>
</template>

<style scoped>
.box {
  max-width: 668px;
}

.manager-card {
  background: white;
  border-radius: 12px;
  box-shadow: var(--card-shadow);
  border: 1px solid rgba(0, 0, 0, 0.05);
  margin-bottom: 2rem;
  overflow: hidden;
}

.card-header-custom {
  background: white;
  padding: 1.5rem;
  border-bottom: 1px solid #f1f5f9;
  display: flex;
  justify-content: space-between;
  align-items: center;
}
</style>
