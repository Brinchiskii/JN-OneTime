import { defineStore } from 'pinia'
import dashboardService from '@/api/DashboardService'
import { ref } from 'vue'
import type { ProjectStat } from '../types'

export const useDashboardStore = defineStore('dashboard', () => {
  
  const ProjectStats = ref<ProjectStat[]>()
  
  const fetchStats = async (managerId: number, startDate: string, endDate: string): Promise<ProjectStat[]> => {
    const res = await dashboardService.getDashboardStats(managerId, startDate, endDate)
    ProjectStats.value = res.data
    return res.data
  }

  return { fetchStats, ProjectStats}
})