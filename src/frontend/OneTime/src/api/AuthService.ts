import type { UserLogin } from '@/types';
import http from './http';

const auths = "/auths"

export default {
    login(email: string, password: string){
        return http.post<UserLogin>(auths + "/login", {email, password})
    }
}