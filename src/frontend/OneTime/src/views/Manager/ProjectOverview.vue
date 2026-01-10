<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useUserStore } from '@/stores/UserStore'
import { useDashboardStore } from '@/stores/DashboardStore'
import type { ProjectStat, User } from '@/types'
import { useAuthStore } from '@/stores/AuthStore'
import dayjs from 'dayjs'
import weekOfYear from 'dayjs/plugin/weekOfYear'
import isoWeek from 'dayjs/plugin/isoWeek'
import 'dayjs/locale/da' 

dayjs.extend(weekOfYear)
dayjs.extend(isoWeek)
dayjs.locale('da')

const AuthStore = useAuthStore()
const dashboardStore = useDashboardStore()
const userStore = useUserStore()

const projectStats = ref<ProjectStat[]>([])
const loading = ref(false)
const employees = ref<User[]>([]) 
const selectedEmployeeId = ref<number>(0)

const currentCursor = ref(dayjs())
const periodMode = ref<string>('month')

const startDate = computed(() => {
  if (periodMode.value === 'all') return '2000-01-01'
  return currentCursor.value.startOf(periodMode.value as any).format('YYYY-MM-DD')
})

const endDate = computed(() => {
  if (periodMode.value === 'all') return dayjs().format('YYYY-MM-DD')
  return currentCursor.value.endOf(periodMode.value as any).format('YYYY-MM-DD')
})

const formattedCursor = computed(() => {
  if (periodMode.value === 'week') return `Uge ${currentCursor.value.isoWeek()}, ${currentCursor.value.format('YYYY')}`
  if (periodMode.value === 'month') return currentCursor.value.format('MMMM YYYY')
  if (periodMode.value === 'year') return currentCursor.value.format('YYYY')
  return 'Hele perioden'
})

const fetchEmployees = async () => {
  try {
    const res = await userStore.getUsersByManagerId(AuthStore.user!.userId)
    employees.value = res.data
  } catch (error) {
    console.error(error)
  }
}

const fetchStats = async () => {
  loading.value = true
  projectStats.value = []

  try {
    if (selectedEmployeeId.value === 0) {
      projectStats.value = await dashboardStore.fetchStats(
        AuthStore.user?.userId ?? 0, 
        startDate.value, 
        endDate.value
      )
    } else {
      const response = await dashboardStore.fetchUserStats(
        selectedEmployeeId.value, 
        startDate.value, 
        endDate.value
      )
      
      projectStats.value = response.map((item: any) => ({
        projectId: item.projectId || 0,
        projectName: item.projectName,
        totalHours: item.hours || item.totalHours, 
        status: 0,
        members: []
      }))
    }
  } catch (error) {
    console.error("Fejl ved hentning af dashboard data", error)
  } finally {
    loading.value = false
  }
}

const moveCursor = (amount: number) => {
  if (periodMode.value === 'all') return
  currentCursor.value = currentCursor.value.add(amount, periodMode.value as any)
}

const goToToday = () => {
  currentCursor.value = dayjs()
}

const onInputSelected = (event: any) => {
  if (!event.target.value) return
  const val = event.target.value

  if (periodMode.value === 'week') {
    const [year, week] = val.split('-W')
    currentCursor.value = dayjs().year(parseInt(year)).isoWeek(parseInt(week)).startOf('week')
  } else if (periodMode.value === 'month') {
    currentCursor.value = dayjs(val)
  } else if (periodMode.value === 'year')
    currentCursor.value = dayjs(val)
}

const totalPeriodHours = computed(() => {
  return projectStats.value!.reduce((sum, p) => sum + p.totalHours, 0)
})

const mostActiveProject = computed(() => {
  if (projectStats.value?.length === 0) return null
  return projectStats.value!.reduce((prev, current) => (prev.totalHours > current.totalHours) ? prev : current)
})

const getPercentageShare = (projectHours: number) => {
  if (totalPeriodHours.value === 0) return 0
  return Math.round((projectHours / totalPeriodHours.value!) * 100)
}

const getAvatarColor = (name: string) => {
  const colors = ['#ef4444', '#f97316', '#10b981', '#3b82f6', '#8b5cf6', '#d946ef', '#059669', '#d97706'];
  let hash = 0;
  for (let i = 0; i < name.length; i++) hash = name.charCodeAt(i) + ((hash << 5) - hash);
  return colors[Math.abs(hash) % colors.length];
}

onMounted(() => { 
  fetchEmployees()
  fetchStats() 
})

watch([currentCursor, periodMode, selectedEmployeeId], () => { 
  fetchStats() 
})
</script>

<template>
  <div class="p-4 p-lg-5 container">

    <div class="d-flex flex-column flex-md-row align-items-end mb-5 gap-3">
      <div>
        <h6 class="text-uppercase text-muted fw-bold mb-2" style="font-size: 0.75rem; letter-spacing: 1px">
          Overblik
        </h6>
        <h2 class="fw-bold mb-0 text-dark">
            {{ selectedEmployeeId ? 'Medarbejder Performance' : 'Team Performance' }}
        </h2>
      </div>

      <div class="d-flex flex-column flex-md-row gap-3">

        <div class="bg-white p-2 rounded shadow-sm border d-flex align-items-center">
            <span class="text-muted small fw-bold text-uppercase px-2" style="font-size: 0.7rem;">Visning:</span>
            
            <select 
                class="form-select border-0 bg-light fw-bold" 
                style="width: auto; min-width: 180px; font-size: 0.9rem;" 
                v-model="selectedEmployeeId"
            >
                <option :value="0">👥 Hele Teamet</option>
                <option disabled>──────────</option>
                <option v-for="emp in employees" :key="emp.userId" :value="emp.userId">
                    👤 {{ emp.name }}
                </option>
            </select>
        </div>

        <div class="d-flex gap-2 align-items-center bg-white p-2 rounded shadow-sm border">

          <select class="form-select border-0 bg-light fw-bold text-uppercase"
            style="width: auto; font-size: 0.8rem; letter-spacing: 0.5px;" v-model="periodMode">
            <option value="week">Uge</option>
            <option value="month">Måned</option>
            <option value="year">År</option>
            <option value="all">Altid</option>
          </select>

          <div class="vr mx-1"></div>

          <div class="btn-group shadow-none" role="group">
            <button class="btn btn-light btn-sm border" @click="moveCursor(-1)" :disabled="periodMode === 'all'">
              <i class="bi bi-chevron-left"></i>
            </button>

            <button class="btn btn-light btn-sm border px-3 fw-medium" @click="goToToday"
              :disabled="periodMode === 'all'">
              I dag
            </button>

            <button class="btn btn-light btn-sm border" @click="moveCursor(1)" :disabled="periodMode === 'all'">
              <i class="bi bi-chevron-right"></i>
            </button>
          </div>

          <div class="position-relative d-flex align-items-center justify-content-center px-2" style="min-width: 120px;">
            
            <span class="fw-bold text-capitalize">{{ formattedCursor }}</span>

            <input v-if="periodMode === 'week'" type="week" class="date-input-hidden" @change="onInputSelected"
              :value="`${currentCursor.isoWeekYear()}-W${String(currentCursor.isoWeek()).padStart(2, '0')}`" />

            <input v-else-if="periodMode === 'month'" type="month" class="date-input-hidden" @change="onInputSelected"
              :value="currentCursor.format('YYYY-MM')" />

            <input v-else-if="periodMode === 'year'" type="year" class="date-input-hidden" @change="onInputSelected"
              :value="currentCursor.format('YYYY')" />
          </div>

          <button class="btn btn-light btn-sm border text-muted" @click="fetchStats">
            <i class="bi bi-arrow-clockwise" :class="{ 'spin-icon': loading }"></i>
          </button>

        </div>
      </div>
    </div>

    <div class="row g-4 mb-5">
      <div class="col-md-6 col-xl-4">
        <div
          class="card border-0 shadow-sm h-100 text-white p-3 overflow-hidden position-relative card-hover bg-primary">
          <div class="position-absolute end-0 bottom-0 opacity-25 me-n3 mb-n3">
            <i class="bi bi-clock-history" style="font-size: 8rem;"></i>
          </div>
          <div class="card-body position-relative z-1 d-flex flex-column justify-content-between">
            <div>
              <h6 class="opacity-75 mb-1">Totale Timer</h6>
              <p class="small opacity-50 mb-0">I valgte periode</p>
            </div>
            <div class="mt-4">
              <h1 class="display-4 fw-bold mb-0">
                <span v-if="loading" class="spinner-border spinner-border-sm"></span>
                <span v-else>{{ totalPeriodHours.toFixed(0) }}<span class="fs-3">t</span></span>
              </h1>
              <p class="mb-0 mt-2 opacity-75">Fordelt på {{ projectStats?.length }} projekter</p>
            </div>
          </div>
        </div>
      </div>
      <div class="col-md-6 col-xl-4">
        <div class="card border-0 shadow-sm h-100 p-3 card-hover bg-white">
          <div class="card-body d-flex flex-column justify-content-between">
            <div class="d-flex align-items-center mb-3">
              <div
                class="rounded-circle bg-success bg-opacity-10 text-success d-flex align-items-center justify-content-center me-3"
                style="width: 48px; height: 48px;">
                <i class="bi bi-graph-up-arrow fs-4"></i>
              </div>
              <div>
                <h6 class="text-muted mb-0">Højest Aktivitet</h6>
                <div class="fw-bold text-truncate" style="max-width: 200px;">
                  {{ mostActiveProject?.projectName || '-' }}
                </div>
              </div>
            </div>

            <div v-if="mostActiveProject" class="mt-3">
              <div class="d-flex justify-content-between align-items-end mb-2">
                <h3 class="fw-bold mb-0">{{ mostActiveProject.totalHours }} <span class="fs-5 text-muted">Timer</span>
                </h3>
                <span class="badge bg-success bg-opacity-10 text-success">
                  {{ getPercentageShare(mostActiveProject.totalHours) }}% af total
                </span>
              </div>
              <div class="progress" style="height: 6px;">
                <div class="progress-bar bg-success" style="width: 100%"></div>
              </div>
            </div>
            <div v-else-if="loading" class="text-center text-muted mt-4">Beregner...</div>
            <div v-else class="text-muted mt-4">Ingen data i perioden</div>
          </div>
        </div>
      </div>
    </div>

    <h5 class="fw-bold mb-3">
        {{ selectedEmployeeId ? 'Medarbejderens Projekter' : 'Projekt Fordeling' }}
    </h5>

    <div v-if="loading && projectStats?.length === 0" class="text-center py-5 my-5 text-muted">
      <div class="spinner-border text-primary mb-3" role="status"></div>
      <div>Henter data...</div>
    </div>

    <div v-else-if="!loading && projectStats?.length === 0" class="text-center py-5 my-5 text-muted bg-light rounded">
      <i class="bi bi-clipboard-x display-4 mb-3 d-block opacity-50"></i>
      Ingen timer registreret i denne periode.
    </div>

    <div v-else class="row g-4">
      <div v-for="project in projectStats" :key="project.projectId" class="col-12 col-xl-6">
        <div class="card border-0 shadow-sm h-100 card-hover">
          <div class="card-body p-4">

            <div class="d-flex justify-content-between align-items-start mb-4">
              <div class="d-flex gap-3 align-items-center">
                <div class="rounded d-flex align-items-center justify-content-center bg-light fw-bold fs-4 text-primary"
                  style="width: 54px; height: 54px;">
                  {{ project.projectName.charAt(0) }}
                </div>
                <div>
                  <h5 class="fw-bold mb-1">{{ project.projectName }}</h5>
                  <span v-if="project.status !== 0"
                    class="badge bg-secondary small py-1 px-2 opacity-75">Arkiveret</span>
                </div>
              </div>
              <div class="text-end">
                <span class="fs-3 fw-bold d-block lh-1">{{ project.totalHours }}</span>
                <span class="text-muted small text-uppercase ls-1">Timer</span>
              </div>
            </div>

            <div class="mb-4">
              <div class="d-flex justify-content-between small mb-2">
                <span class="fw-bold text-muted text-uppercase" style="font-size: 0.7rem; letter-spacing: 0.5px;">Andel
                  af arbejdsbyrde</span>
                <span class="fw-bold text-primary">
                  {{ getPercentageShare(project.totalHours) }}%
                </span>
              </div>
              <div class="progress" style="height: 8px; border-radius: 10px; background-color: #f1f5f9;">
                <div class="progress-bar" role="progressbar"
                  :style="{ width: getPercentageShare(project.totalHours) + '%', opacity: 0.8 }"></div>
              </div>
            </div>

            <div v-if="selectedEmployeeId === 0" class="d-flex align-items-center justify-content-between border-top pt-3">
              <span class="text-muted small fw-bold text-uppercase"
                style="font-size: 0.7rem; letter-spacing: 0.5px;">Bidragydere</span>

              <div class="d-flex ps-2 align-items-center">
                <div v-for="(member, index) in project.members" :key="member.userId"
                  class="avatar-small rounded-circle border border-2 border-white d-flex align-items-center justify-content-center text-white fw-bold shadow-sm"
                  :style="{
                    backgroundColor: getAvatarColor(member.name),
                    width: '32px', height: '32px', fontSize: '0.75rem',
                    marginLeft: '-8px',
                    zIndex: 10 - index,
                    cursor: 'help'
                  }" data-bs-toggle="tooltip" data-bs-placement="top"
                  :title="member.name + ': ' + member.hoursContributor + ' timer'">
                  {{ member.name.charAt(0) }}
                </div>
                <div v-if="project.members.length === 0" class="text-muted small fst-italic ms-2">Ingen endnu</div>
              </div>
            </div>

          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.card-hover {
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}

.card-hover:hover {
  transform: translateY(-3px);
  box-shadow: 0 10px 20px -5px rgba(0, 0, 0, 0.08) !important;
}

.spin-icon {
  animation: spin 0.5s linear;
}

@keyframes spin {
  100% {
    transform: rotate(360deg);
  }
}

input[type="date"]::-webkit-inner-spin-button,
input[type="date"]::-webkit-calendar-picker-indicator {
  cursor: pointer;
  opacity: 0.6;
}

input[type="date"]:hover::-webkit-calendar-picker-indicator {
  opacity: 1;
}

.date-input-hidden {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  opacity: 0;
  cursor: pointer;
  z-index: 10;
}
</style>