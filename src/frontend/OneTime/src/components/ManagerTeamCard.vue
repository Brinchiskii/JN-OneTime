<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useTimesheetStore } from '../stores/timesheetStore'
import Timesheet from './Timesheet.vue'
import type { TimesheetRow } from '@/types'

const props = defineProps<{
  userName: string
  rows: TimesheetRow
  status: number
}>()

const statusBadge = computed(() => {
  switch (props.status) {
    case 0:
      return { text: "Afventer", class: "badge bg-warning" }
    case 1:
      return { text: "Godkendt", class: "badge bg-success" }
    case 2:
      return { text: "Afvist", class: "badge bg-danger" }
    case 3:
      return { text: "Kladde", class: "badge bg-secondary" }
    default:
      return { text: "Ikke oprettet", class: "badge bg-dark" }
  }
})

const timesheetStore = useTimesheetStore()
const comment = ref("")

const approve = () => {
  if (!props.rows?.timesheetId) return
  timesheetStore.submitDecision(props.rows.timesheetId, 1, comment.value)
  alert("Timesheet " + props.rows.timesheetId + " er blevet godkendt")
  emit('refresh')
}

const deny = () => {
  if (!props.rows?.timesheetId) return
  timesheetStore.submitDecision(props.rows.timesheetId, 2, comment.value)
  alert("Timesheet " + props.rows.timesheetId + " er blevet afvist")
  emit('refresh')
}

const emit = defineEmits<{
  (e: 'refresh'): void
}>()
</script>

<template>
  <div class="manager-card">
    <div class="card-header-custom">
      <div class="d-flex align-items-center gap-3">
        <div class="avatar">{{ userName.charAt(0) }}</div>
        <div class="fw-bold fs-5">{{ userName }}</div>
        <div :class="statusBadge.class">{{ statusBadge.text }}</div>
      </div>
      <div v-if="props.status == 0" class="d-flex align-items-center">
        <input v-model="comment" type="text" placeholder="Tilføj kommentar..." />
        <button class="btn btn-success btn-sm px-3 rounded-pill ms-2" @click="approve()">
          <i class="bi bi-check-lg me-1"></i> <span>Godkend</span>
        </button>
        <button class="btn btn-danger btn-sm px-3 rounded-pill ms-2" @click="deny()">
          <i class="bi bi-check-lg me-1"></i> <span>Afvis</span>
        </button>
      </div>
    </div>
    <Timesheet :timesheetrows="rows" :weekDays="timesheetStore.weekDays" :readonly="true"></Timesheet>
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
