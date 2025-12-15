import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/', redirect: '/login' },
    { path: '/login', name: 'login', component: () => import('@/views/Login.vue') },
    {
      path: '/admin',
      meta: { requiresAuth: true, role: 0},
      component: () => import('@/views/Admin/AdminDashboard.vue'),
      children: [
        {
          path: 'users',
          name: 'admin-users',
          meta: { requiresAuth: true, role: 0},
          component: () => import('@/views/Admin/ManageUsers.vue'),
        },
        {
          path: 'logs',
          name: 'admin-logs',
          meta: { requiresAuth: true, role: 0},
          component: () => import('@/views/Admin/AuditLogs.vue'),
        },
      ],
    },

    {
      path: '/manager',
      name: 'manager',
      meta: { requiresAuth: true, role: 1},
      component: () => import('@/views/ManagerDashboard.vue'),
    },

    {
      path: '/employee',
      name: 'employee',
      meta: { requiresAuth: true, role: 2},
      component: () => import('@/views/EmployeeDashboard.vue'),
    },
  ],
})

router.beforeEach((to) => {
  const token = localStorage.getItem('token')
  const role = Number(localStorage.getItem('role'))

  if(to.meta.requiresAuth && !token){
    return '/login'
  }

  if(to.meta.role !== undefined && to.meta.role !== role){
    return '/login'
  }
})

export default router
