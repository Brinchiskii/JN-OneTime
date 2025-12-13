import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { type User } from '@/types'

export const useAuthStore = defineStore('auth', () => {
  const user = ref<User | null>(null)

  const isAuthenticated = computed(() => !!user.value)

  const userRole = computed(() => user.value?.role)

  const login = (userData: User) => {
    user.value = userData
  }

  const logout = () => {
    user.value = null
  }

  return { user, isAuthenticated, userRole, login, logout }
})
