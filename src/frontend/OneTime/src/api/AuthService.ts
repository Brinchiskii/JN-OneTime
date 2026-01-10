import type { UserLogin } from '@/types';
import https from './Https';

const auths = "/auths"

export default {
    login(email: string, password: string){
        return https.post<UserLogin>(auths + "/login", {email, password})
    }
}