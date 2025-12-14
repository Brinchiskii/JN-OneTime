import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/', redirect: '/login' },
    { path: '/login', name: 'login', component: () => import('@/views/Login.vue') },
    {
      path: '/admin',
      component: () => import('@/views/Admin/AdminDashboard.vue'),
      children: [
        {
          path: 'users',
          name: 'admin-users',
          component: () => import('@/views/Admin/ManageUsers.vue'),
        },
        {
          path: 'logs',
          name: 'admin-logs',
          component: () => import('@/views/Admin/AuditLogs.vue'),
        },
      ],
    },

    {
      path: '/manager',
      name: 'manager',
      component: () => import('@/views/ManagerDashboard.vue'),
    },

    {
      path: '/employee',
      name: 'employee',
      component: () => import('@/views/EmployeeDashboard.vue'),
    },
  ],
})

export default router
