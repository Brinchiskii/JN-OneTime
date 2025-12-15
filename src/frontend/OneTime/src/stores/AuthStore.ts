import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { type UserLogin } from '@/types'
import AuthService from '@/api/AuthService'
import { useRouter } from 'vue-router'

export const useAuthStore = defineStore('auth', () => {
  const user = ref<UserLogin | null>(null)

  const isAuthenticated = computed(() => !!user.value)

  const userRole = computed(() => user.value?.role)

  const login = async (email: string, password: string) => {
    const res = await AuthService.login(email, password)
    user.value = res.data
  }

  const router = useRouter()

  const logout = () => {
    user.value = null
    localStorage.removeItem('token')
    localStorage.removeItem('role')
    router.push('/login')
  }

  const roleText = computed(() => {
    switch(userRole.value){
        case 0: return "Admin"
        case 1: return "Leder"
        case 2: return "Medarbejder"
    }
})

  return { user, isAuthenticated, userRole, login, logout, roleText }
})
