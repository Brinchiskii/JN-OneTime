<script setup lang="ts">
import { onMounted, ref, computed } from 'vue'
import { useTimesheetStore } from '../stores/TimesheetStore'
import { useProjectStore } from '../stores/ProjectStore'
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
        class: "btn-warning",     
        icon: "bi bi-hourglass-split", 
        isDisabled: true         
      };
    case 1: // Godkendt
      return {
        text: "Godkendt",
        class: "btn-success",     
        icon: "bi bi-check-circle-fill", 
        isDisabled: true          
      };
    case 2: // Afvist
      return {
        text: "Afvist - Send igen", 
        class: "btn-danger",     
        icon: "bi bi-exclamation-circle-fill", 
        isDisabled: false         
      };
    default: // Ikke sendt (null eller andet)
      return {
        text: "Indsend ugeskema",
        class: "btn-primary",     
        icon: "bi bi-send-fill", 
        isDisabled: false
      };
  }
});

const loading = () => {
  isLoading.value = true
  timesheetStore.GetTimesheet()
    .finally(() => {
      isLoading.value = false
    })
}

onMounted(() => {
  projectStore.fetchProjects()
  loading()
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
    <div class="d-flex justify-content-between align-items-center mb-1">
      <DatePicker @change="loading"></DatePicker>
      <transition name="fade">
        <div 
            v-if="timesheetStore.currentComment" 
            class="alert alert-warning shadow-sm d-flex align-items-start gap-3 border-0 border-start border-4 border-warning" 
            role="alert"
        >
            <div class="text-warning mt-1">
                <i class="bi bi-chat-quote-fill fs-4"></i>
            </div>  
            
            <div>
                <h6 class="alert-heading fw-bold mb-1 text-dark">
                    Besked fra din leder
                </h6>
                <p class="mb-0 text-dark opacity-75">
                    {{ timesheetStore.currentComment }}
                </p>
            </div>
        </div>
    </transition>
    </div>
    <div class="card bg-light mb-4 p-3">
      <div class="d-flex justify-content-between align-items-center">
        <span>Uge Total: <strong>{{ grandTotal }}t</strong></span>
        <span class="small">Standard: 37 timer/uge</span>
      </div>
    </div>

    <span class="spinner-border spinner-border-sm" v-if="isLoading"></span>
    <span class="ms-2" v-if="isLoading">Loading...</span>
    <Timesheet v-else :timesheetrows="timesheetStore.myRows" :weekDays="timesheetStore.weekDays"
      :readonly="timesheetStore.currentTimesheetStatus === 1 || timesheetStore.currentTimesheetStatus === 0"></Timesheet>
  </div>
</template>

<style scoped></style>
