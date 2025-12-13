import http from './http';
import { type Log } from '@/types'

export default {
    getLogs() {
        return http.get<Log[]>("/auditlogs/all");
    }
}