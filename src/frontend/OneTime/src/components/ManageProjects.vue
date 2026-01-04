<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useProjectStore } from '@/stores/ProjectStore'
import type { Project } from '@/types'

const props = defineProps<{
  isAdmin: boolean 
}>()

const projectStore = useProjectStore()
const searchQuery = ref('')
const sortStatus = ref(0)

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

const filteredProjects = computed(() => {
    let data = projectStore.projects

    if (searchQuery.value) {
        const query = searchQuery.value.toLowerCase()
        data = data.filter(p => p.name.toLowerCase().includes(query))
    }

    if (sortStatus.value === 1) data = data.filter(p => p.status === 0) // Kun aktive
    if (sortStatus.value === 2) data = data.filter(p => p.status === 1) // Kun arkiverede
    if (sortStatus.value === 3) data = data.filter(p => p.status === 2) // Kun arkiverede
    if (sortStatus.value === 4) data = data.filter(p => p.status === 3) // Kun arkiverede

    return data.sort((a, b) => {
        let compareA: any = a[sortColumn.value as keyof Project]
        let compareB: any = b[sortColumn.value as keyof Project]

        if (typeof compareA === 'string') compareA = compareA.toLowerCase()
        if (typeof compareB === 'string') compareB = compareB.toLowerCase()

        if (compareA < compareB) return sortDirection.value === 'asc' ? -1 : 1
        if (compareA > compareB) return sortDirection.value === 'asc' ? 1 : -1
        return 0
    })
})

const showCreateModal = ref(false)
const showEditModal = ref(false)
const showDeleteModal = ref(false)
const loading = ref(false)

const emptyProject = { name: '', description: '', status: 0, startDate: new Date().toISOString().split('T')[0] }
const projectForm = ref<Partial<Project>>({ ...emptyProject })
const selectedProject = ref<Project | null>(null)

const openCreateModal = () => {
    projectForm.value = { ...emptyProject }
    showCreateModal.value = true
}

const createProject = async () => {
    loading.value = true
    try {
        await projectStore.createProject(projectForm.value)
        showCreateModal.value = false
        await projectStore.fetchProjects()
    } finally {
        loading.value = false
    }
}

const openEditModal = (project: Project) => {
    if(!props.isAdmin) return
    selectedProject.value = project
    projectForm.value = { ...project }
    showEditModal.value = true
}

const updateProject = async () => {
    if (!selectedProject.value?.projectId || !props.isAdmin) return

    loading.value = true
    try {
        await projectStore.updateProject(projectForm.value)
        showEditModal.value = false
        await projectStore.fetchProjects()
    } finally {
        loading.value = false
    }
}

const openDeleteModal = (project: Project) => {
    if(!props.isAdmin) return
    selectedProject.value = project
    showDeleteModal.value = true
}

const deleteProject = async () => {
    if (!selectedProject.value?.projectId || !props.isAdmin) return
    await projectStore.deleteProject(selectedProject.value.projectId)
    showDeleteModal.value = false
    await projectStore.fetchProjects()
}

const getStatusBadge = (status: number) => {
    if (status === 0) return { text: 'Aktiv', class: 'bg-success text-white' }
    if (status === 1) return { text: 'På pause', class: 'bg-warning text-white' }
    if (status === 2) return { text: 'Afsluttet', class: 'bg-dark text-white' }
    if (status === 3) return { text: 'Arkiveret', class: 'bg-secondary text-white' }
    return { text: 'Ukendt', class: 'bg-light text-muted border' }
}

onMounted(() => {
    projectStore.fetchProjects()
})
</script>

<template>
    <div class="flex-grow-1 p-4 p-lg-5 overflow-auto card h-100 border-0 shadow-none bg-transparent">

        <div class="d-flex justify-content-between align-items-center mb-5">
            <div>
                <h6 class="text-uppercase text-muted fw-bold mb-2" style="font-size: 0.75rem; letter-spacing: 1px">
                    Projekter
                </h6>
                <h2 class="fw-bold mb-0 text-dark">Administrer projekter</h2>
            </div>
            <button class="btn btn-primary-admin" @click="openCreateModal">
                <i class="bi bi-plus-lg me-2"></i> Opret Nyt Projekt
            </button>
        </div>

        <div class="admin-card p-0 mb-4">

            <div class="p-3 border-bottom d-flex gap-3 bg-light bg-opacity-50">
                <input type="text" class="search-input" placeholder="Søg projekt..." v-model="searchQuery" />
                <select class="form-select w-auto border-light shadow-sm bg-white" v-model="sortStatus">
                    <option :value="0">Vis Alle</option>
                    <option :value="1">Kun Aktive</option>
                    <option :value="2">På pause</option>
                    <option :value="3">Afsluttet</option>
                    <option :value="4">Arkiveret</option>
                </select>
            </div>

            <table class="table-admin">
                <thead>
                    <tr>
                        <th @click="sortTable('name')" style="cursor: pointer;">
                            Projekt Navn
                            <span v-if="sortColumn === 'name'" class="text-primary">
                                <i
                                    :class="sortDirection === 'asc' ? 'bi bi-sort-alpha-down ms-1' : 'bi bi-sort-alpha-up-alt ms-1'"></i>
                            </span>
                            <span v-else class="text-muted opacity-25"><i class="bi bi-arrow-down-up"></i></span>
                        </th>
                        <th @click="sortTable('status')" style="cursor: pointer;">
                            Status
                            <span v-if="sortColumn === 'status'" class="text-primary">
                                <i
                                    :class="sortDirection === 'asc' ? 'bi bi-caret-up-fill ms-1' : 'bi bi-caret-down-fill ms-1'"></i>
                            </span>
                            <span v-else class="text-muted opacity-25"><i class="bi bi-arrow-down-up"></i></span>
                        </th>
                        <th class="text-end" v-if="isAdmin">Handlinger</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="project in filteredProjects" :key="project.projectId">
                        <td>
                            <div class="d-flex align-items-center gap-3">
                                <div class="rounded d-flex align-items-center justify-content-center bg-primary bg-opacity-10 text-primary fw-bold"
                                    style="width: 40px; height: 40px;">
                                    {{ project.name.charAt(0).toUpperCase() }}
                                </div>
                                <div class="fw-bold">{{ project.name }}</div>
                            </div>
                        </td>
                        <td>
                            <span class="badge rounded-pill fw-normal px-3 py-2 shadow-sm"
                                :class="getStatusBadge(project.status).class">
                                {{ getStatusBadge(project.status).text }}
                            </span>
                        </td>

                        <td class="text-end" v-if="isAdmin">
                            <button class="btn btn-light btn-sm me-1 text-muted" @click="openEditModal(project)">
                                <i class="bi bi-pencil"></i>
                            </button>
                            <button class="btn btn-light btn-sm text-danger" @click="openDeleteModal(project)">
                                <i class="bi bi-trash3"></i>
                            </button>
                        </td>
                    </tr>
                </tbody>
            </table>

            <div v-if="filteredProjects.length === 0" class="text-center py-5 text-muted">
                <i class="bi bi-folder2-open display-4 mb-3 d-block opacity-50"></i>
                Ingen projekter fundet
            </div>
        </div>
    </div>

    <Teleport to="body">
        <div v-if="showCreateModal" class="modal-overlay" @click.self="showCreateModal = false">
            <form class="modal-card" @submit.prevent="createProject">
                <div class="modal-header">
                    <h5 class="mb-0 fw-bold">Nyt Projekt</h5>
                    <button type="button" class="btn-close" @click="showCreateModal = false"></button>
                </div>
                <div class="modal-body">
                    <div class="row g-3">
                        <div class="col-12">
                            <label class="form-label">Projekt Navn</label>
                            <input type="text" class="form-control" v-model="projectForm.name" required
                                placeholder="F.eks. Hjemmeside Redesign" />
                        </div>
                        <div class="col-6">
                            <label class="form-label">Status</label>
                            <select class="form-select" v-model="projectForm.status">
                                <option :value="0">Aktiv</option>
                                <option :value="1">På pause</option>
                                <option :value="2">Afsluttet</option>
                                <option :value="3">Arkiveret</option>
                            </select>
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-light border"
                        @click="showCreateModal = false">Annuller</button>
                    <button type="submit" class="btn btn-primary-admin" :disabled="loading">
                        <span v-if="loading" class="spinner-border spinner-border-sm me-2"></span>
                        Opret Projekt
                    </button>
                </div>
            </form>
        </div>
    </Teleport>

    <Teleport to="body">
        <div v-if="showEditModal" class="modal-overlay" @click.self="showEditModal = false">
            <form class="modal-card" @submit.prevent="updateProject">
                <div class="modal-header">
                    <h5 class="mb-0 fw-bold">Rediger Projekt</h5>
                    <button type="button" class="btn-close" @click="showEditModal = false"></button>
                </div>
                <div class="modal-body">
                    <div class="row g-3">
                        <div class="col-12">
                            <label class="form-label">Projekt Navn</label>
                            <input type="text" class="form-control" v-model="projectForm.name" required />
                        </div>
                        <div class="col-6">
                            <label class="form-label">Status</label>
                            <select class="form-select" v-model="projectForm.status">
                                <option :value="0">Aktiv</option>
                                <option :value="1">På pause</option>
                                <option :value="2">Afsluttet</option>
                                <option :value="3">Arkiveret</option>
                            </select>
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-light border" @click="showEditModal = false">Annuller</button>
                    <button type="submit" class="btn btn-primary-admin" :disabled="loading">Gem Ændringer</button>
                </div>
            </form>
        </div>
    </Teleport>

    <Teleport to="body">
        <div v-if="showDeleteModal" class="modal-overlay" @click.self="showDeleteModal = false">
            <div class="modal-card" style="max-width: 400px">
                <div class="modal-body text-center pt-4">
                    <div class="rounded-circle bg-danger bg-opacity-10 d-inline-flex align-items-center justify-content-center mb-3"
                        style="width: 60px; height: 60px">
                        <i class="bi bi-exclamation-triangle text-danger fs-3"></i>
                    </div>
                    <h5 class="fw-bold mb-2">Slet Projekt?</h5>
                    <p class="text-muted mb-0">
                        Er du sikker på, at du vil slette <strong>{{ selectedProject?.name }}</strong>?<br>
                        Dette kan påvirke tidsregistreringer.
                    </p>
                </div>
                <div class="modal-footer border-0 pt-0 pb-4 justify-content-center">
                    <button type="button" class="btn btn-light border px-4"
                        @click="showDeleteModal = false">Annuller</button>
                    <button type="button" class="btn btn-danger px-4" @click="deleteProject">Slet Projekt</button>
                </div>
            </div>
        </div>
    </Teleport>

</template>

<style scoped>
.admin-card {
    background: white;
    border-radius: 12px;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
    border: 1px solid rgba(0, 0, 0, 0.05);
    overflow: hidden;
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

/* Custom Button */
.btn-primary-admin {
    background-color: #0f172a;
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

/* Search input */
.search-input {
    border: 1px solid #e2e8f0;
    padding: 8px 16px;
    border-radius: 8px;
    background-color: white;
    width: 250px;
}

.search-input:focus {
    outline: none;
    border-color: #0f172a;
}

/* Modals */
.modal-overlay {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background-color: rgba(0, 0, 0, 0.5);
    backdrop-filter: blur(4px);
    z-index: 9999;
    display: flex;
    align-items: center;
    justify-content: center;
}

.modal-card {
    background: white;
    width: 90%;
    max-width: 500px;
    border-radius: 12px;
    box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1);
    overflow: hidden;
}

.modal-header {
    padding: 1.25rem;
    border-bottom: 1px solid #f1f5f9;
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.modal-body {
    padding: 1.25rem;
}

.modal-footer {
    padding: 1rem 1.25rem;
    background-color: #f8fafc;
    border-top: 1px solid #f1f5f9;
    display: flex;
    justify-content: flex-end;
    gap: 10px;
}

.form-label {
    font-size: 0.85rem;
    font-weight: 600;
    color: #475569;
    margin-bottom: 0.4rem;
}

.form-control,
.form-select {
    border: 1px solid #cbd5e1;
    padding: 0.6rem;
    border-radius: 6px;
    font-size: 0.95rem;
}

.form-control:focus,
.form-select:focus {
    border-color: #0f172a;
    box-shadow: 0 0 0 2px rgba(15, 23, 42, 0.1);
}
</style>