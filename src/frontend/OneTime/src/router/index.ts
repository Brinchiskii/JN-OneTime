import { createRouter, createWebHistory } from 'vue-router'
import EmployeeDashboardView from '../views/EmployeeDashboard.vue'
import ManagerDashboardView from '../views/ManagerDashboard.vue'
import LoginView from '../views/Login.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/', redirect: '/login' },
    { path: '/login', component: LoginView },
    { path: '/manager', component: ManagerDashboardView },
    { path: '/employee', component: EmployeeDashboardView },
  ],
})

export default router
