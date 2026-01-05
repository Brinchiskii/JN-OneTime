<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useUserStore } from '@/stores/UserStore'
import type { User, UserPayload } from '@/types'

const userStore = useUserStore()

const sortRole = ref(4)
const searchQuery = ref('')
const sortColumn = ref('name')
const sortDirection = ref('asc')

const sortTable = (column: string) => {
  if (sortColumn.value === column) {
    sortDirection.value = sortDirection.value === 'asc' ? 'desc' : 'asc'
  } else {
    sortColumn.value = column
    sortDirection.value = 'asc'
  }
}

const filteredUsers = computed(() => {
  let data = userStore.users
  if (sortRole.value < 4) data = data.filter((user) => user.role === sortRole.value)
  if (searchQuery.value) {
    const query = searchQuery.value.toLowerCase()
    data = data.filter(
      (user) => user.name.toLowerCase().includes(query) || user.email.toLowerCase().includes(query),
    )
  }
  return data.sort((a, b) => {
    let modifier = sortDirection.value === 'asc' ? 1 : -1

    let valA: any = ''
    let valB: any = ''
    switch (sortColumn.value) {
      case 'name':
        valA = a.name.toLowerCase()
        valB = b.name.toLowerCase()
        break
      case 'email':
        valA = a.email.toLowerCase()
        valB = b.email.toLowerCase()
        break
      case 'role':
        valA = a.role
        valB = b.role
        break
      case 'manager':
        valA = a.managerId ? userStore.getNameById(a.managerId).toLowerCase() : ''
        valB = b.managerId ? userStore.getNameById(b.managerId).toLowerCase() : ''
        break
      default:
        valA = a.name
        valB = b.name
    }

    if (valA < valB) return -1 * modifier
    if (valA > valB) return 1 * modifier
    return 0
  })
})

const newUser = ref<UserPayload>({ name: '', email: '', password: null, managerId: null, role: 0 })
const userSelected = ref<User>()
const showCreateModal = ref(false)
const showDeleteModal = ref(false)
const showUpdateModal = ref(false)
const isLoading = ref(false)

const openCreateModal = () => {
  showCreateModal.value = true
  newUser.value = { name: '', email: '', password: null, managerId: null, role: 0 }
}

const createUser = async () => {
  try {
    isLoading.value = true
    if (newUser.value.role == 2 && !newUser.value.managerId) {
      alert("En medarbejder skal have en manager tilknyttet.")
      return
    }
    await userStore.createUser(newUser.value)
    showCreateModal.value = false
    userStore.fetchUsers()
    alert(newUser.value.name + " er nu blevet tilføjet")
  } catch (error) {
    console.error('Fejl under oprettelse:', error)
    alert('Der opstod en fejl under oprettelse af brugeren. Prøv igen.')
  }
  finally {
    isLoading.value = false
  }
}

const chooseDelete = (id: number) => {
  userSelected.value = userStore.users.find((user) => user.userId === id)
  showDeleteModal.value = true
}
const deleteUser = async () => {
  if (userSelected.value) await userStore.deleteUserById(userSelected.value?.userId)
  showDeleteModal.value = false
  userStore.fetchUsers()
  alert(userSelected.value?.name + " er nu blevet slettet")
}

const chooseUpdate = (id: number) => {
  userSelected.value = userStore.users.find(user => user.userId === id)
  if (userSelected.value)
    newUser.value = {
      name: userSelected.value.name,
      email: userSelected.value.email,
      password: "",
      managerId: userSelected.value.managerId,
      role: userSelected.value.role
    }
  showUpdateModal.value = true
}

const updateUser = async () => {
  if (newUser.value.role == 2 && !newUser.value.managerId) {
    alert("En medarbejder skal have en manager tilknyttet.")
    return
  }
  if (newUser.value.role != 2) {
    newUser.value.managerId = null
  }
  try {
    isLoading.value = true
    if (userSelected.value) await userStore.updateUser(userSelected.value.userId, newUser.value)
    showUpdateModal.value = false
    userStore.fetchUsers()
    alert(newUser.value.name + " er blevet opdateret")
  } catch (error) {
    console.error('Fejl under opdatering:', error)
    alert('Der opstod en fejl under opdatering af brugeren. Prøv igen.')
  }
  finally {
    isLoading.value = false
  }
}

const roleText = (roleId: number) => {
  switch (roleId) {
    case 0: return "Admin"
    case 1: return "Leder"
    case 2: return "Medarbejder"
  }
}

const managers = computed(() => userStore.users.filter(user => user.role === 1))

onMounted(() => {
  userStore.fetchUsers()
})

</script>
<template>
  <div class="flex-grow-1 p-4 p-lg-5 overflow-auto card">
    <div class="d-flex justify-content-between align-items-center mb-5">
      <div>
        <h6 class="text-uppercase text-muted fw-bold mb-2" style="font-size: 0.75rem; letter-spacing: 1px">
          Brugere
        </h6>
        <h2 class="fw-bold mb-0 text-dark">Administrer Konti</h2>
      </div>
      <button class="btn btn-primary-admin" @click="openCreateModal">
        <i class="bi bi-plus-lg me-2"></i> Opret Ny Bruger
      </button>
    </div>

    <div class="admin-card p-0 mb-4">
      <div class="p-3 border-bottom d-flex gap-3 bg-light bg-opacity-50">
        <input type="text" class="search-input" placeholder="Søg efter navn eller email..." v-model="searchQuery" />
        <select class="form-select w-auto border-light shadow-sm" style="background: white" v-model="sortRole">
          <option :value="4">Alle Roller</option>
          <option :value="0">Admin</option>
          <option :value="1">Leder</option>
          <option :value="2">Medarbejder</option>
        </select>
      </div>

      <table class="table-admin">
        <thead>
          <tr>
            <th @click="sortTable('name')" class="cursor-pointer user-select-none">
              Bruger
              <span v-if="sortColumn === 'name'" class="text-primary">
                <i :class="sortDirection === 'asc' ? 'bi bi-sort-alpha-down' : 'bi bi-sort-alpha-up-alt'"></i>
              </span>
              <span v-else class="text-muted opacity-25"><i class="bi bi-arrow-down-up"></i></span>
            </th>

            <th @click="sortTable('email')" class="cursor-pointer user-select-none">
              Email
              <span v-if="sortColumn === 'email'" class="text-primary">
                <i :class="sortDirection === 'asc' ? 'bi bi-sort-alpha-down' : 'bi bi-sort-alpha-up-alt'"></i>
              </span>
              <span v-else class="text-muted opacity-25"><i class="bi bi-arrow-down-up"></i></span>
            </th>

            <th @click="sortTable('role')" class="cursor-pointer user-select-none">
              Rolle
              <span v-if="sortColumn === 'role'" class="text-primary">
                <i :class="sortDirection === 'asc' ? 'bi bi-sort-alpha-down' : 'bi bi-sort-alpha-up-alt'"></i>
              </span>
              <span v-else class="text-muted opacity-25"><i class="bi bi-arrow-down-up"></i></span>
            </th>

            <th @click="sortTable('manager')" class="cursor-pointer user-select-none">
              Leder
              <span v-if="sortColumn === 'manager'" class="text-primary">
                <i :class="sortDirection === 'asc' ? 'bi bi-sort-alpha-down' : 'bi bi-sort-alpha-up-alt'"></i>
              </span>
              <span v-else class="text-muted opacity-25"><i class="bi bi-arrow-down-up"></i></span>
            </th>

            <th class="text-end">Handlinger</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="user in filteredUsers" :key="user.userId">
            <td>
              <div class="d-flex align-items-center gap-3">
                <div class="fw-bold">{{ user.name }}</div>
              </div>
            </td>
            <td class="text-muted">{{ user.email }}</td>
            <td>
              <span class="badge-role" :class="'role-' + roleText(user.role)">
                {{ roleText(user.role) }}
              </span>
            </td>
            <td>
              <span v-if="user.managerId" class="badge-role text-dark border shadow-sm">
                {{ userStore.getNameById(user.managerId) }}
              </span>

              <span v-else class="text-muted small ms-2">-</span>
            </td>
            <td class="text-end">
              <button class="btn btn-light btn-sm me-1 text-muted" @click="chooseUpdate(user.userId)">
                <i class="bi bi-pencil"></i>
              </button>
              <button class="btn btn-light btn-sm text-danger" @click="chooseDelete(user.userId)">
                <i class="bi bi-trash3"></i>
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
  <div v-if="showCreateModal" class="modal-overlay" @click.self="showCreateModal = false">
    <form class="modal-card" @submit.prevent="createUser">
      <div class="modal-header">
        <h5 class="mb-0 fw-bold">Opret Ny Bruger</h5>
        <button class="btn-close" @click="showCreateModal = false"></button>
      </div>

      <div class="modal-body">
        <div class="row g-3">
          <div class="col-12">
            <label class="form-label">Fuldt Navn</label>
            <input type="text" class="form-control" v-model="newUser.name" placeholder="F.eks. Louise Andersen"
              required />
          </div>
          <div class="col-12">
            <label class="form-label">Email Adresse</label>
            <input type="email" class="form-control" v-model="newUser.email" placeholder="navn@firma.dk" required />
          </div>
          <div class="col-12">
            <label class="form-label">Kodeord</label>
            <input type="password" class="form-control" v-model="newUser.password" required />
          </div>
          <div class="col-md-6">
            <label class="form-label">Rolle</label>
            <select class="form-select" v-model="newUser.role" reqiured>
              <option :value="2">Medarbejder</option>
              <option :value="1">Manager</option>
              <option :value="0">Admin</option>
            </select>
          </div>
          <div class="col-md-6" :hidden="newUser.role != 2">
            <label class="form-label">Vælg Manager</label>
            <select class="form-select" v-model="newUser.managerId">
              <option :value="null">Ingen manager valgt</option>

              <option v-for="manager in managers" :key="manager.userId" :value="manager.userId">
                {{ manager.name }}
              </option>
            </select>
          </div>
        </div>
      </div>

      <div class="modal-footer">
        <button class="btn btn-light border" @click="showCreateModal = false">Annuller</button>
        <button type="submit" class="btn btn-primary-admin" :disabled="isLoading">
          <span v-if="isLoading" class="spinner-border spinner-border-sm me-2"></span>
          {{ isLoading ? 'Opretter bruger...' : 'Opret Bruger' }}
        </button>
      </div>
    </form>
  </div>

  <div v-if="showUpdateModal" class="modal-overlay" @click.self="showUpdateModal = false">
    <form class="modal-card" @submit.prevent="updateUser">
      <div class="modal-header">
        <h5 class="mb-0 fw-bold">Opdater Bruger</h5>
        <button class="btn-close" @click="showUpdateModal = false"></button>
      </div>

      <div class="modal-body">
        <div class="row g-3">
          <div class="col-12">
            <label class="form-label">Fuldt Navn</label>
            <input type="text" class="form-control" v-model="newUser.name" placeholder="F.eks. Louise Andersen"
              required />
          </div>
          <div class="col-12">
            <label class="form-label">Email Adresse</label>
            <input type="email" class="form-control" v-model="newUser.email" placeholder="navn@firma.dk" required />
          </div>
          <div class="col-md-6">
            <label class="form-label">Rolle</label>
            <select class="form-select" v-model="newUser.role" reqiured>
              <option :value="2">Medarbejder</option>
              <option :value="1">Manager</option>
              <option :value="0">Admin</option>
            </select>
          </div>
          <div class="col-md-6" :hidden="newUser.role != 2">
            <label class="form-label">Vælg Manager</label>
            <select class="form-select" v-model="newUser.managerId">
              <option :value="null">Ingen manager valgt</option>

              <option v-for="manager in managers" :key="manager.userId" :value="manager.userId">
                {{ manager.name }}
              </option>
            </select>
          </div>
        </div>
      </div>

      <div class="modal-footer">
        <button class="btn btn-light border" @click="showUpdateModal = false">Annuller</button>
        <button type="submit" class="btn btn-primary-admin" :disabled="isLoading">
          <span v-if="isLoading" class="spinner-border spinner-border-sm me-2"></span>
          {{ isLoading ? 'Opdaterer bruger...' : 'Opdater bruger' }}
        </button>
      </div>
    </form>
  </div>

  <div v-if="showDeleteModal" class="modal-overlay" @click.self="showDeleteModal = false">
    <div class="modal-card" style="max-width: 400px">
      <div class="modal-body text-center pt-4">
        <div class="rounded-circle bg-danger bg-opacity-10 d-inline-flex align-items-center justify-content-center mb-3"
          style="width: 60px; height: 60px">
          <i class="bi bi-exclamation-triangle text-danger fs-3"></i>
        </div>
        <h5 class="fw-bold mb-2">Slet Bruger?</h5>
        <p class="text-muted mb-0">
          Er du sikker på, at du vil slette <strong>{{ userSelected?.name }}</strong>? Denne handling kan ikke
          fortrydes.
        </p>
      </div>
      <div class="modal-footer border-0 pt-0 pb-4 justify-content-center">
        <button type="button" class="btn btn-light border px-4" @click="showDeleteModal = false">
          Annuller
        </button>
        <button type="button" class="btn btn-danger px-4" @click="deleteUser">Slet Bruger</button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.admin-card {
  background: white;
  border-radius: 12px;
  box-shadow: var(--card-shadow);
  border: 1px solid rgba(0, 0, 0, 0.05);
  padding: 1.5rem;
  height: 100%;
}

.table-admin {
  width: 100%;
  border-collapse: separate;
  border-spacing: 0;
}

.table-admin th {
  background-color: #f8fafc;
  color: #64748b;
  font-size: 0.75rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  padding: 16px;
  font-weight: 600;
  border-bottom: 1px solid #e2e8f0;
  text-align: left;
}

.table-admin td {
  padding: 16px;
  border-bottom: 1px solid #f1f5f9;
  vertical-align: middle;
  color: #334155;
}

.table-admin tr:last-child td {
  border-bottom: none;
}

.table-admin tr:hover td {
  background-color: #fafafa;
}

.badge-role {
  padding: 4px 12px;
  /* Lidt bredere for pænere look */
  border-radius: 20px;
  font-size: 0.75rem;
  font-weight: 600;
  color: white;
  /* Tvinger hvid tekst */
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  /* Lille skygge for effekt */
}

.role-Admin {
  background-color: green;
  /* Dyb Lilla */
}

.role-Leder {
  background-color: #6366f1;
  /* Stærk Blå */
}

.role-Medarbejder {
  background-color: #475569;
  /* Mørk Koksgrå */
}

.badge-status {
  padding: 4px 10px;
  border-radius: 6px;
  font-size: 0.75rem;
  font-weight: 600;
}

.status-active {
  background-color: #dcfce7;
  color: #166534;
}

.status-inactive {
  background-color: #fee2e2;
  color: #991b1b;
}

.action-create {
  color: #166534;
}

.action-delete {
  color: #991b1b;
}

.action-update {
  color: #854d0e;
}

.btn-primary-admin {
    background-color: var(--primary-color);
    color: white;
    border: none;
    padding: 10px 20px;
    border-radius: 8px;
    font-weight: 500;
    transition: all 0.2s;
}

.btn-primary-admin:hover {
    background-color: #1e293b;
    transform: translateY(-1px);
}

.search-input {
  border: 1px solid #e2e8f0;
  padding: 10px 16px;
  border-radius: 8px;
  background-color: white;
  width: 300px;
}

.search-input:focus {
  outline: none;
  border-color: var(--primary-color);
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(0, 0, 0, 0.4);
  backdrop-filter: blur(4px);
  z-index: 1000;
  display: flex;
  align-items: center;
  justify-content: center;
  animation: fadeIn 0.2s ease-out;
}

.modal-card {
  background: white;
  width: 100%;
  max-width: 500px;
  border-radius: 16px;
  box-shadow:
    0 20px 25px -5px rgba(0, 0, 0, 0.1),
    0 10px 10px -5px rgba(0, 0, 0, 0.04);
  overflow: hidden;
  animation: slideUp 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

.modal-header {
  padding: 1.5rem 1.5rem 1rem;
  border-bottom: 1px solid #f1f5f9;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.modal-body {
  padding: 1.5rem;
}

.modal-footer {
  padding: 1rem 1.5rem;
  background-color: #f8fafc;
  border-top: 1px solid #f1f5f9;
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}

.form-label {
  font-weight: 500;
  font-size: 0.9rem;
  color: #475569;
  margin-bottom: 6px;
}

.form-control,
.form-select {
  border: 1px solid #cbd5e1;
  padding: 10px;
  border-radius: 8px;
  font-size: 0.95rem;
}

.form-control:focus,
.form-select:focus {
  border-color: var(--primary-color);
  box-shadow: 0 0 0 3px rgba(63, 87, 117, 0.1);
}

@keyframes fadeIn {
  from {
    opacity: 0;
  }

  to {
    opacity: 1;
  }
}

@keyframes slideUp {
  from {
    transform: translateY(20px);
    opacity: 0;
  }

  to {
    transform: translateY(0);
    opacity: 1;
  }
}
</style>
