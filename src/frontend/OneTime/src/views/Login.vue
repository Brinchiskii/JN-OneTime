<script setup lang="ts">
import SelectRole from '@/components/SelectRole.vue'
import { ref } from 'vue'
import { useAuthStore } from '@/stores/AuthStore'
import { useRouter } from 'vue-router'

const AuthStore = useAuthStore()
const router = useRouter()

const roleSelected = ref('')
const selectRole = (role: string) => {
  roleSelected.value = role
}

const loading = ref(false)
const email = ref('')
const password = ref('')
const rememberMe = ref(false)

const login = async () => {
  try {
    loading.value = true
    await AuthStore.login(email.value, password.value, rememberMe.value)
    const user = AuthStore.user
    if (user) {
      if (user.role === 0) router.push('/admin')
      if (user.role === 1) router.push('/manager')
      if (user.role === 2) router.push('/employee')
    }

  } catch (error) {
    console.error('Login failed:', error)
    alert('Konto findes ikke, prøv igen.')
  } finally {
    loading.value = false
  }
}

</script>

<template>
  <SelectRole v-if="!roleSelected" @select-role="selectRole"></SelectRole>

  <div v-else class="container d-flex justify-content-center align-items-center vh-100">
    <div class="card select-role-card shadow">
      <div class="card-body text-center p-4">
        <div class="d-grid gap-3 mx-auto mt-4">
          <div key="login" class="d-flex flex-column h-100">
            <div class="back-link" @click="roleSelected = ''">
              <i class="bi bi-arrow-left me-2"></i> Tilbage
            </div>

            <h5 class="header-title">{{ roleSelected }} Login</h5>

            <form @submit.prevent="login" class="mt-2">
              <div>
                <label class="form-label small fw-bold text-muted" style="font-size: 0.75rem">EMAIL</label>
                <input type="email" class="form-control" v-model="email" placeholder="name@company.com" required />
              </div>

              <div>
                <label class="form-label small fw-bold text-muted" style="font-size: 0.75rem">PASSWORD</label>
                <input type="password" class="form-control" v-model="password" placeholder="••••••••" required />
              </div>

              <div class="d-flex justify-content-between align-items-center mb-4">
                <div class="form-check">
                  <input class="form-check-input" type="checkbox" v-model="rememberMe" id="remember" style="border-color: #d1d5db" />
                  <label class="form-check-label small text-muted" for="remember">Husk mig</label>
                </div>
              </div>

              <button type="submit" class="btn-submit" :disabled="loading">
                <span v-if="loading" class="spinner-border spinner-border-sm me-2"></span>
                {{ loading ? 'Logger ind...' : 'Log ind' }}
              </button>
            </form>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.form-control {
  padding: 0.75rem 1rem;
  border-radius: 6px;
  border: 1px solid #d1d5db;
  margin-bottom: 1rem;
}

.form-control:focus {
  border-color: var(--primary-dark);
  box-shadow: 0 0 0 1px var(--primary-dark);
}

.btn-submit {
  width: 100%;
  padding: 0.85rem;
  border-radius: 6px;
  font-weight: 500;
  margin-top: 1rem;
  transition: background 0.2s;
  border: none;
}

.back-link {
  color: #6b7280;
  text-decoration: none;
  font-size: 0.9rem;
  display: inline-flex;
  align-items: center;
  margin-bottom: 1.5rem;
  cursor: pointer;
  transition: color 0.2s;
}

.back-link:hover {
  color: #111827;
}

.fade-enter-active,
.fade-leave-active {
  transition:
    opacity 0.2s ease,
    transform 0.2s ease;
}

.fade-enter-from {
  opacity: 0;
  transform: translateY(5px);
}

.fade-leave-to {
  opacity: 0;
  transform: translateY(-5px);
}

.select-role-card {
  max-width: 500px;
  width: 100%;
}
</style>
