import { createRouter, createWebHistory } from 'vue-router'
import EmployeeDashboardView from '../views/EmployeeDashboard.vue'
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/', name: 'timesheet', component: EmployeeDashboardView },
  ],
})

export default router
