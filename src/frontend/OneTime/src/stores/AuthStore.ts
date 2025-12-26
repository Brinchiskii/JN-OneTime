import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { UserLogin, JwtPayload } from '@/types'
import AuthService from '@/api/AuthService'
import { jwtDecode } from 'jwt-decode'

export const useAuthStore = defineStore('auth', () => {
  const user = ref<UserLogin | null>(null)

  const isAuthenticated = computed(() => !!user.value)

  const userRole = computed(() => user.value?.role)

  const login = async (email: string, password: string, rememberMe: boolean) => {
    try{

      const res = await AuthService.login(email, password)
      const token = res.data.token     
      localStorage.removeItem('token')
      sessionStorage.removeItem('token')

      if (rememberMe) {
        localStorage.setItem('token', token)
      } else {
        sessionStorage.setItem('token', token)
      }
      
      setUserFromToken(token)
      
    } catch (error) {
      throw error
    }
  }

  const setUserFromToken = (token: string) => {
    try {
      const decoded = jwtDecode<JwtPayload>(token)

      // Tjek udløb
      if (decoded.exp < Date.now() / 1000) {
        logout()
        return
      }

      user.value = {
        userId: Number(decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"]),
        name: decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"],
        role: Number(decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"]),
        email: decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"],
        token: token
      }

    } catch (error) {
      console.error("Fejl i token decode", error)
      logout()
    }
  }

  const logout = () => {
    user.value = null
    localStorage.removeItem('token')
    sessionStorage.removeItem('token')
    window.location.href = '/login'
  }

  const roleText = computed(() => {
    switch (userRole.value) {
      case 0: return "Admin"
      case 1: return "Leder"
      case 2: return "Medarbejder"
    }
  })

  const initializeAuth = () => {
    const token = sessionStorage.getItem('token') || localStorage.getItem('token')
    if (token) setUserFromToken(token)
  }

  return { user, isAuthenticated, userRole, login, logout, roleText, initializeAuth, }
})
