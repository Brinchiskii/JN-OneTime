import https from './Https'
import type { UserPayload, User } from '@/types'

const users = '/users'
export default {
  getUsers() {
    return https.get<User[]>(users)
  },

  createUser(payload: UserPayload) {
    return https.post<UserPayload>(users, payload)
  },

  deleteUserById(id: number) {
    return https.delete(`${users}/${id}`)
  },

  updateUser(id: number, payload: UserPayload) {
    return https.put(`${users}/${id}`, payload)
  },

  getUsersByManagerId(managerId: number) {
    return https.get<User[]>(`${users}/leader/${managerId}`)
  }
}
