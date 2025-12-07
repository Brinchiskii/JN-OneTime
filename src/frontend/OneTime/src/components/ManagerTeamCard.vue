<script setup lang="ts">
import { ref, computed } from 'vue'
import { useTimesheetStore } from '../stores/timesheetStore'
import Timesheet from './Timesheet.vue'
import type { TimesheetRow } from '@/types'

// Vi tager imod data fra forælderen
const props = defineProps<{
  userName: string
  rows: TimesheetRow[]
}>()

const timesheetStore = useTimesheetStore()
const comment = ref("")

const timesheetId = computed(() => {
    return 1
})

const approve = () => {
  if(!timesheetId.value) return
  timesheetStore.submitDecision(timesheetId.value, 1, comment.value)
}

const deny = () => {
  if(!timesheetId.value) return
  timesheetStore.submitDecision(timesheetId.value, 2, comment.value)
}
</script>

<template>
  <div class="manager-card">
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
      <div>
        <input v-model="comment" type="text" placeholder="Tilføj kommentar..." />
        <button class="btn btn-success btn-sm px-3 rounded-pill ms-2" @click="approve()">
          <i class="bi bi-check-lg me-1"></i> <span>Godkend</span>
        </button>
        <button class="btn btn-danger btn-sm px-3 rounded-pill ms-2" @click="deny()">
          <i class="bi bi-check-lg me-1"></i> <span>Afvis</span>
        </button>
      </div>
    </div>

    <Timesheet :rows="rows" :weekDays="timesheetStore.weekDays" :readonly="true"></Timesheet>
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
