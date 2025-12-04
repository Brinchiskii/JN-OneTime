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
  <div class="container my-4">
    <div class="bg-white border-bottom pb-3">
      <h5 class="my-1">Denne uge</h5>
      <p class="text-muted small mb-0">{{ timesheetStore.weekHeader }}</p>
    </div>

    
      <!-- Week Total -->
      <div class="card bg-light mb-4 p-3">
        <div class="d-flex justify-content-between align-items-center">
          <span>Uge Total: <strong>0t</strong></span>
          <span class="small">Standard: 37 timer/uge</span>
        </div>
      </div>
      
    <Timesheet
      :rows="timesheetStore.myRows"
      :weekDays="timesheetStore.weekDays"
        :projects="projectStore.projects"
    ></Timesheet>

      
  </div>
  
</template>

<style scoped></style>
