<script setup lang="ts">
import { ref, computed } from 'vue'
import { useTimesheetStore } from '../stores/TimesheetStore'
import Timesheet from './Timesheet.vue'
import type { TimesheetRow } from '@/types'

const props = defineProps<{
  userName: string
  rows: TimesheetRow
  status: number
  comment: string | null
}>()

const emit = defineEmits<{
  (e: 'refresh'): void
}>()

const timesheetStore = useTimesheetStore()
const comment = ref("")
const isEditing = ref(false) 

const Status = computed(() => {
  switch (props.status) {
    case 0: return { text: "Afventer", class: "bg-warning text-dark", icon: "bi bi-hourglass-split" };
    case 1: return { text: "Godkendt", class: "bg-success", icon: "bi bi-check-circle-fill" };
    case 2: return { text: "Afvist", class: "bg-danger", icon: "bi bi-exclamation-circle-fill" };
    case 3: return { text: "Kladde", class: "bg-secondary", icon: "bi bi-pencil-square" };
    default: return { text: "Ikke oprettet", class: "bg-light text-muted border", icon: "bi bi-circle" };
  }
});

const startEdit = () => {
  isEditing.value = true
  timesheetStore.updateRows(props.rows)
}

const cancelEdit = () => {
  isEditing.value = false
}

const saveChanges = async () => {
  try {
    const res = await timesheetStore.saveTimesheet(false)
    isEditing.value = false
    res ?? alert("Ændringer er gemt. Du kan nu godkende.")
  } catch (e) {
    alert("Fejl ved gem: " + e)
  } finally {
    emit('refresh')
  }
}

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
</script>

<template>
  <div class="manager-card">
    <div class="card-header-custom">

      <div class="d-flex align-items-center gap-3">
        <div class="avatar">{{ userName.charAt(0) }}</div>
        <div class="fw-bold fs-5">{{ userName }}</div>
        <span class="badge d-inline-flex align-items-center gap-1 py-2 px-3" :class="Status.class">
          <i :class="Status.icon"></i>
          <span>{{ Status.text }}</span>
        </span>
      </div>

      <div v-if="props.status != 1" class="d-flex align-items-center gap-2">

        <template v-if="!isEditing">
          <input v-model="comment" type="text" class="form-control form-control-sm" placeholder="Tilføj kommentar..."
            style="width: 200px;" />

          <button class="btn btn-success btn-sm px-3" @click="approve()">
            <i class="bi bi-check-lg me-1"></i> Godkend
          </button>

          <button class="btn btn-danger btn-sm px-3" @click="deny()">
            <i class="bi bi-x-lg me-1"></i> Afvis
          </button>

          <div class="vr mx-1"></div> <button class="btn btn-outline-primary btn-sm px-3" @click="startEdit()">
            <i class="bi bi-pencil me-1"></i> Ret
          </button>
        </template>

        <template v-else>
          <span class="text-muted small me-2 fst-italic">Redigerer timesheet...</span>
          <button class="btn btn-primary btn-sm px-3" @click="saveChanges()">
            <i class="bi bi-save me-1"></i> Gem ændringer
          </button>
          <button class="btn btn-secondary btn-sm px-3" @click="cancelEdit()">
            Annuller
          </button>
        </template>

      </div>

      <div v-if="props.comment && !isEditing">{{ props.comment }}</div>
    </div>

    <Timesheet :timesheetrows="rows" :weekDays="timesheetStore.weekDays" :readonly="!isEditing"></Timesheet>

  </div>
</template>

<style scoped>
.manager-card {
  background: white;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
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

.avatar {
  width: 32px;
  height: 32px;
  background-color: #e9ecef;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
  color: #495057;
}
</style>