<script setup lang="ts">
import { onMounted, ref, computed } from 'vue'
import { useTimesheetStore } from '../stores/timesheetStore'
import { useProjectStore } from '../stores/projectStore'
import Timesheet from './Timesheet.vue'
import DatePicker from './DatePicker.vue'
import { isBuildCommand } from 'typescript'

const timesheetStore = useTimesheetStore()
const projectStore = useProjectStore()
const isSubmitting = ref(false)
const isLoading = ref(true)

const handleSubmit = async (submit: boolean) => {
  isSubmitting.value = true
  try {
    await timesheetStore.saveTimesheet(submit)
  } catch (error) {
    console.error('Fejl under indsendelse:', error)
  } finally {
    isSubmitting.value = false
  }
}

const getRowTotal = (row: any) => {
  if (!row || !row.hours) return 0

  const total = Object.values(row.hours).reduce((acc: any, val: any) => {
    return acc + (Number(val) || 0)
  }, 0)

  return total
}

const grandTotal = computed(() => {
  return timesheetStore.myRows.rows.map(row => getRowTotal(row)).reduce((acc: any, val: any) => acc + val, 0)
})

const Status = computed(() => {
  switch (timesheetStore.currentTimesheetStatus) {
    case 0: // Afventer
      return {
        text: "Afventer godkendelse",
        class: "btn-warning",     // Gul farve
        icon: "bi bi-hourglass-split", // Timeglas ikon
        isDisabled: true          // Man kan typisk ikke sende igen, mens man venter
      };
    case 1: // Godkendt
      return {
        text: "Godkendt",
        class: "btn-success",     // Grøn farve
        icon: "bi bi-check-circle-fill", // Flueben ikon
        isDisabled: true          // Låst fordi den er godkendt
      };
    case 2: // Afvist
      return {
        text: "Afvist - Send igen", // Opfordring til handling
        class: "btn-danger",      // Rød farve
        icon: "bi bi-exclamation-circle-fill", // Advarsels ikon
        isDisabled: false         // Åben så man kan rette og sende igen
      };
    default: // Ikke sendt (null eller andet)
      return {
        text: "Indsend ugeskema",
        class: "btn-primary",     // Blå farve (Standard handling)
        icon: "bi bi-send-fill",  // Send/Papirflyver ikon
        isDisabled: false
      };
  }
});

onMounted(() => {
  isLoading.value = true
  projectStore.fetchProjects()
  timesheetStore.GetTimesheet()
    .finally(() => {
      isLoading.value = false
    })
})
</script>

<template>
  <div class="container my-4">
    <div class="card mb-3 p-3 flex-row justify-content-between align-items-center">

      <h2 class="text-muted mb-0">{{ timesheetStore.weekHeader }}</h2>
      <div class="d-flex gap-2">
        <button @click="handleSubmit(false)"
          :hidden="timesheetStore.currentTimesheetStatus === 1 || timesheetStore.currentTimesheetStatus === 0 || isSubmitting"
          class="btn btn-secondary">Gem som kladde
        </button>

        <button :class="Status.class + ' btn d-flex align-items-center'" @click="handleSubmit(true)"
          :disabled="timesheetStore.currentTimesheetStatus === 1 || timesheetStore.currentTimesheetStatus === 0 || isSubmitting">
          <span v-if="isSubmitting" class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
          <i v-else :class="Status.icon"></i>
          <span v-if="isSubmitting" class="ms-2">Indsender...</span>
          <span v-else class="ms-2">{{ Status.text }}</span>
        </button>
      </div>
    </div>
    <DatePicker @click="timesheetStore.GetTimesheet" @change="timesheetStore.GetTimesheet"></DatePicker>
    <div class="card bg-light mb-4 p-3">
      <div class="d-flex justify-content-between align-items-center">
        <span>Uge Total: <strong>{{ grandTotal }}t</strong></span>
        <span class="small">Standard: 37 timer/uge</span>
      </div>
    </div>

    <span v-if="isLoading">Loading...</span>
    <Timesheet v-else :timesheetrows="timesheetStore.myRows" :weekDays="timesheetStore.weekDays"
      :readonly="timesheetStore.currentTimesheetStatus === 1 || timesheetStore.currentTimesheetStatus === 0"></Timesheet>
  </div>
</template>

<style scoped></style>
