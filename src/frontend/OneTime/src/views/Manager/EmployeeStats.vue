<script setup lang="ts">
import { ref, onMounted, watch } from 'vue'
import { useAuthStore } from '@/stores/AuthStore'
import axios from 'axios'
import dayjs from 'dayjs'
import { useUserStore } from '@/stores/UserStore'
import type { User } from '@/types'

const authStore = useAuthStore()
const userStore = useUserStore()
const loading = ref(false)
const stats = ref<any[]>([])
const employees = ref<User[]>([])
const selectedUserId = ref<number | null>(null)
const totalHours = ref(0)

// Standard periode: Denne måned
const startDate = ref(dayjs().startOf('month').format('YYYY-MM-DD'))
const endDate = ref(dayjs().endOf('month').format('YYYY-MM-DD'))

// 1. Hent listen over medarbejdere (Teamet)
const fetchEmployees = async () => {
    try {
        // Vi bruger dit eksisterende endpoint til at hente brugere under en manager
        // (Justér stien hvis den hedder noget andet i din Service)
        await userStore.fetchUsers()
        employees.value = userStore.users
    } catch (e) {
        console.error("Kunne ikke hente medarbejdere", e)
    }
}

// 2. Hent stats for den VALGTE bruger
const fetchStats = async () => {
    if (!selectedUserId.value) return

    loading.value = true
    try {
        // Her bruger vi ID'et fra dropdownen i stedet for AuthStore
        const response = await axios.get(`http://localhost:5273/api/dashboard/stats/user/${selectedUserId.value}`, {
            params: { startDate: startDate.value, endDate: endDate.value }
        })
        
        stats.value = response.data
        totalHours.value = stats.value.reduce((sum, item) => sum + item.hours, 0)
        
    } catch (e) {
        console.error(e)
    } finally {
        loading.value = false
    }
}

const getBarWidth = (hours: number) => {
    if (totalHours.value === 0) return '0%'
    return ((hours / totalHours.value) * 100) + '%'
}

// Hent medarbejdere når siden loader
onMounted(() => {
    fetchEmployees()
})

// Hent nye stats hvis man ændrer datoerne, men kun hvis en bruger er valgt
watch([startDate, endDate], () => {
    if (selectedUserId.value) fetchStats()
})
</script>

<template>
<div class="container py-4">
    
    <div class="d-flex justify-content-between align-items-center mb-4">
        <h2 class="fw-bold">Medarbejder Indsigt</h2>
    </div>

    <div class="card shadow-sm mb-4 border-0">
        <div class="card-body bg-light rounded">
            <div class="row g-3 align-items-end">
                
                <div class="col-md-4">
                    <label class="form-label small fw-bold text-muted">VÆLG MEDARBEJDER</label>
                    <select v-model="selectedUserId" @change="fetchStats" class="form-select">
                        <option :value="null" disabled>-- Vælg en ansat --</option>
                        <option v-for="emp in employees" :key="emp.userId" :value="emp.userId">
                            {{ emp.name }}
                        </option>
                    </select>
                </div>

                <div class="col-md-3">
                    <label class="form-label small fw-bold text-muted">FRA</label>
                    <input type="date" v-model="startDate" class="form-control">
                </div>
                <div class="col-md-3">
                    <label class="form-label small fw-bold text-muted">TIL</label>
                    <input type="date" v-model="endDate" class="form-control">
                </div>
                
                <div class="col-md-2">
                    <button class="btn btn-primary w-100" @click="fetchStats" :disabled="!selectedUserId">
                        <i class="bi bi-search me-1"></i> Vis
                    </button>
                </div>
            </div>
        </div>
    </div>

    <div v-if="!selectedUserId" class="text-center py-5 text-muted">
        <i class="bi bi-people display-4 mb-3 d-block"></i>
        <p class="lead">Vælg en medarbejder ovenfor for at se deres statistik.</p>
    </div>

    <div v-else-if="loading" class="text-center py-5">
        <div class="spinner-border text-primary" role="status"></div>
    </div>

    <div v-else>
        <div class="row mb-4">
            <div class="col-md-12">
                <div class="card shadow-sm border-0 bg-primary text-white">
                    <div class="card-body d-flex justify-content-between align-items-center p-4">
                        <div>
                            <h6 class="text-white-50 text-uppercase mb-1">Total Timer ({{ startDate }} - {{ endDate }})</h6>
                            <h1 class="display-4 fw-bold mb-0">{{ totalHours.toFixed(1) }}</h1>
                        </div>
                        <i class="bi bi-person-lines-fill display-1 opacity-25"></i>
                    </div>
                </div>
            </div>
        </div>

        <div class="card shadow border-0">
            <div class="card-header bg-white py-3">
                <h5 class="mb-0 fw-bold">Projektfordeling</h5>
            </div>
            <div class="card-body">
                
                <div v-if="stats.length === 0" class="text-center text-muted py-4">
                    Ingen registreringer fundet for denne medarbejder i perioden.
                </div>

                <div v-for="(project, index) in stats" :key="index" class="mb-4">
                    <div class="d-flex justify-content-between mb-1">
                        <span class="fw-bold">{{ project.projectName }}</span>
                        <span class="text-muted small">{{ project.hours }} timer ({{ ((project.hours / totalHours) * 100).toFixed(0) }}%)</span>
                    </div>
                    <div class="progress" style="height: 12px; border-radius: 6px;">
                        <div 
                            class="progress-bar bg-dark" 
                            role="progressbar" 
                            :style="{ width: getBarWidth(project.hours) }"
                        ></div>
                    </div>
                </div>

            </div>
        </div>
    </div>
</div>
</template>

<style scoped>
.opacity-25 { opacity: 0.25; }
</style>