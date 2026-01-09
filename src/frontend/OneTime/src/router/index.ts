import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/AuthStore'


const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/', redirect: '/login' },
    { path: '/login', name: 'login', component: () => import('@/views/Login.vue'),
      beforeEnter: (to, from, next) => {
        const AuthStore = useAuthStore()
        AuthStore.initializeAuth()
        if (AuthStore.isAuthenticated) {
          if (AuthStore.userRole === 0) return next('/admin')
          if (AuthStore.userRole === 1) return next('/manager')
          if (AuthStore.userRole === 2) return next('/employee')
        }
        next()
      }
     },
    {
      path: '/admin',
      meta: { requiresAuth: true, role: 0 },
      component: () => import('@/views/Admin/AdminDashboard.vue'),
      redirect: '/admin/users',
      children: [
        {
          path: 'users',
          name: 'admin-users',
          meta: { requiresAuth: true, role: 0 },
          component: () => import('@/views/Admin/ManageUsers.vue'),
        },
        {
          path: 'logs',
          name: 'admin-logs',
          meta: { requiresAuth: true, role: 0 },
          component: () => import('@/views/Admin/AuditLogs.vue'),
        },
        {
          path: 'projects',
          name: 'admin-projects',
          meta: { requiresAuth: true, role: 0 },
          component: () => import('@/views/Admin/ManageProjects.vue'),
        },
      ],
    },

    {
      path: '/manager',
      name: 'manager',
      meta: { requiresAuth: true, role: 1 },
      component: () => import('@/views/Manager/ManagerDashboard.vue'),
      redirect: '/manager/team-timesheets',
      children: [
        {
          path: 'projects',
          name: 'manager-projects',
          meta: { requiresAuth: true, role: 1 },
          component: () => import('@/views/Manager/ManageProjects.vue'),
        },
        {
          path: 'team-timesheets',
          name: 'manager-team-timesheets',
          meta: { requiresAuth: true, role: 1 },
          component: () => import('@/views/Manager/TeamTimesheets.vue'),
        },
        {
          path: 'project-overview',
          name: 'manager-project-overview',
          meta: { requiresAuth: true, role: 1 },
          component: () => import('@/views/Manager/ProjectOverview.vue'),
        }
      ],
    },

    {
      path: '/employee',
      name: 'employee',
      meta: { requiresAuth: true, role: 2 },
      component: () => import('@/views/Employee/EmployeeDashboard.vue'),
      redirect: '/employee/register',
      children: [
        {
          path: 'register',
          name: 'register-time',
          meta: { requiresAuth: true, role: 2},
          component: () => import('@/views/Employee/RegisterTime.vue'),
        },
        {
          path: 'stats',
          name: 'my-stats',
          meta: { requiresAuth: true, role: 2},
          component: () => import('@/views/Employee/MyStats.vue'),
        }
      ]
    },
  ],
})

router.beforeEach((to) => {
  const token = localStorage.getItem('token') || sessionStorage.getItem('token')
  const AuthStore = useAuthStore()
  AuthStore.initializeAuth()

  if (to.meta.requiresAuth && !token) {
    return '/login'
  }

  if (to.meta.role !== undefined && AuthStore.userRole !== to.meta.role) {
    return '/login'
  }
})

export default router
