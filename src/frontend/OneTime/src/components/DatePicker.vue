<script setup lang="ts">
import { useTimesheetStore } from '@/stores/TimesheetStore';
const timesheetStore = useTimesheetStore()


const prevWeek = () => {
    timesheetStore.previousWeek()
}

const nextWeek = () => {
    timesheetStore.nextWeek()
}

const goToToday = () => {
    timesheetStore.setWeek()
}
const onDateSelected = (event: any) => {
    timesheetStore.setWeekFromDate(event.target.value)
}
</script>

<template>
  <span class="d-flex gap-2 my-3">
    <div class="btn-group shadow-sm rounded-3 me-2" role="group">
      <button class="btn btn-white bg-white border" @click="prevWeek" title="Forrige uge">
        <i class="bi bi-chevron-left"></i>
      </button>

      <button class="btn btn-white bg-white border px-3 fw-medium" @click="goToToday">I dag</button>

      <button class="btn btn-white bg-white border" @click="nextWeek" title="Næste uge">
        <i class="bi bi-chevron-right"></i>
      </button>
    </div>

    <div
      class="btn btn-white bg-white border shadow-sm date-picker-wrapper d-flex align-items-center gap-2"
    >
      <i class="bi bi-calendar3"></i>
      <span>Vælg dato</span>
      <input type="date" class="date-input-hidden" @change="onDateSelected" />
    </div>
    <button class="btn btn-white bg-white border">Opdater</button>
  </span>
</template>
