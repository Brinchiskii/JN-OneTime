<script setup lang="ts">
import { onMounted } from 'vue'
import { useTimesheetStore } from '../stores/timesheetStore'
import { useProjectStore } from '../stores/projectStore'
import Timesheet from './Timesheet.vue'

const timesheetStore = useTimesheetStore()
const projectStore = useProjectStore()

onMounted(() => {
  projectStore.fetchProjects()
})
</script>

<template>
  <div class="container card shadow my-4">
    <div class="card-header bg-white border-bottom pb-3">
      <h5 class="my-1">Denne uge</h5>
      <p class="text-muted small mb-0">{{ timesheetStore.weekHeader }}</p>
    </div>

    <!-- Card Body -->
    <div class="card-body">
      <!-- Week Total -->
      <div class="card bg-light mb-4 p-3">
        <div class="d-flex justify-content-between align-items-center">
          <span>Uge Total: <strong>0t</strong></span>
          <span class="small">Standard: 37 timer/uge</span>
        </div>
      </div>

      <Timesheet
        :rows="timesheetStore.rows"
        @delete-row="timesheetStore.removeRow"
        :weekDays="timesheetStore.weekDays"
        :projects="projectStore.projects"
      ></Timesheet>

      <button @click="timesheetStore.addRow" class="btn btn-outline-secondary w-100 mt-3">
        <i class="bi bi-plus-lg me-1"></i> Tilføj nyt projekt
      </button>
    </div>
  </div>
</template>

<style scoped></style>
