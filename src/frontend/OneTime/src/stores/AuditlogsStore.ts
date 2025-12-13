import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import AuditlogsService from '@/api/AuditlogsService'
import { type Log } from '@/types'

export const useAuditLogsStore = defineStore('auditlogs', () => {
  const logs = ref<Log[]>([])
  
  const getLogs = async () => {
    const res = await AuditlogsService.getLogs()
    logs.value = res.data
}
  return { logs, getLogs }
})