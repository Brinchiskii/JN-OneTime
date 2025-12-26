<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useTimesheetStore } from '../stores/timesheetStore'
import { useProjectStore } from '../stores/projectStore'
import Timesheet from './Timesheet.vue'
import DatePicker from './DatePicker.vue'

const timesheetStore = useTimesheetStore()
const projectStore = useProjectStore()
const isSubmitting = ref(false)
const status = ref('DRAFT')

const handleSubmit = async () => {
  isSubmitting.value = true

  try {
    await timesheetStore.submitTimesheet()
    alert('Timesheet indsendt succesfuldt!')
  } catch (error) {
    console.error('Fejl under indsendelse:', error)
  } finally {
    isSubmitting.value = false
  }
}

onMounted(() => {
  projectStore.fetchProjects()
})
</script>

<template>
  <div class="container my-4">
    <div class="bg-white border-bottom pb-3">
      <h5 class="my-1">Denne uge</h5>
      <p class="text-muted mb-0">{{ timesheetStore.weekHeader }}</p>
      <button
        class="btn btn-primary-custom d-flex align-items-center gap-2 shadow-sm"
        @click="handleSubmit"
      >
        <span
          v-if="isSubmitting"
          class="spinner-border spinner-border-sm"
          role="status"
          aria-hidden="true"
        ></span>
        <i v-else class="bi" :class="status === 'DRAFT' ? 'bi-send-fill' : 'bi-check-lg'"></i>
        <span>Indsend</span>
      </button>
    </div>
    <DatePicker></DatePicker>
    <div class="card bg-light mb-4 p-3">
      <div class="d-flex justify-content-between align-items-center">
        <span>Uge Total: <strong>0t</strong></span>
        <span class="small">Standard: 37 timer/uge</span>
      </div>
    </div>

    <Timesheet :rows="timesheetStore.myRows" :weekDays="timesheetStore.weekDays"></Timesheet>
  </div>
</template>

<style scoped></style>
