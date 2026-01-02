import http from './http'
import type { UserPayload, User } from '@/types'

const users = '/users'
export default {
  getUsers() {
    return http.get<User[]>(users)
  },

  createUser(payload: UserPayload) {
    return http.post<UserPayload>(users, payload)
  },

  deleteUserById(id: number) {
    return http.delete(`${users}/${id}`)
  },

  updateUser(id: number, payload: UserPayload) {
    return http.put(`${users}/${id}`, payload)
  },

  getUsersByManagerId(managerId: number) {
    return http.get<User[]>(`${users}/leader/${managerId}`)
  }
}
