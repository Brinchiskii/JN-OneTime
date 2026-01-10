import https from './Https';
import { type Log } from '@/types'

export default {
    getLogs() {
        return https.get<Log[]>("/auditlogs/all");
    }
}