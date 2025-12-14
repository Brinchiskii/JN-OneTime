import { defineStore } from 'pinia'
import userService from '../api/UserService'
import { ref } from 'vue'
import type { User, UserPayload } from '../types'

export const useUserStore = defineStore('user', () => {
  
  const users = ref<User[]>([])
  
  const fetchUsers = async () => {
    const res = await userService.getUsers()
    users.value = res.data
  }

  const createUser = (user: UserPayload) => {
    return userService.createUser(user);
  }

  const deleteUserById = (id: number) => {
    return userService.deleteUserById(id)
  }

  const updateUser = (id: number, payload: UserPayload) => {
    return userService.updateUser(id, payload)
  }

  return { users, fetchUsers, createUser, deleteUserById, updateUser }
})