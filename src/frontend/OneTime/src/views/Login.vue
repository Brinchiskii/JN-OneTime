<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '@/stores/AuthStore'
import { useRouter } from 'vue-router'

const AuthStore = useAuthStore()
const router = useRouter()

const loading = ref(false)
const email = ref('')
const password = ref('')
const rememberMe = ref(false)
const showPassword = ref(false)
const errorMessage = ref('')

const clearError = () => {
  errorMessage.value = ''
}

const login = async () => {
  errorMessage.value = ''
  try {
    loading.value = true
    await AuthStore.login(email.value, password.value, rememberMe.value)

    const user = AuthStore.user
    if (user) {
      if (user.role === 0) router.push('/admin')
      else if (user.role === 1) router.push('/manager')
      else if (user.role === 2) router.push('/employee')
      else router.push('/')
    }

  } catch (error) {
    console.error('Login failed:', error)
    errorMessage.value = 'Forkert email eller adgangskode. Prøv igen.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="container d-flex justify-content-center align-items-center vh-100">
    <div class="card login-card shadow">
      <div class="card-body p-4 p-md-5">

        <div class="text-center mb-5">
          <h3 class="fw-bold">Velkommen til JN OneTime</h3>
          <p class="text-muted">Log ind for at fortsætte</p>
        </div>

        <form @submit.prevent="login">

          <div class="floating-group" :class="{ filled: email.length > 0, 'has-error': errorMessage }">
            <input v-model="email" type="email" required autocomplete="email" id="emailInput" @focus="clearError" />
            <label for="emailInput">Email</label>
          </div>

          <div class="floating-group" :class="{ filled: password.length > 0, 'has-error': errorMessage }">
            <input v-model="password" :type="showPassword ? 'text' : 'password'" required
              autocomplete="current-password" id="passwordInput" class="password-input" @focus="clearError" />
            <label for="passwordInput">Adgangskode</label>

            <i class="bi password-toggle" :class="showPassword ? 'bi-eye' : 'bi-eye-slash'"
              @click="showPassword = !showPassword" role="button"></i>
          </div>

          <div class="d-flex justify-content-between align-items-center mb-4 mt-3">
            <div class="form-check">
              <input class="form-check-input" type="checkbox" v-model="rememberMe" id="remember"
                style="border-color: #d1d5db; cursor: pointer;" />
              <label class="form-check-label small text-muted" for="remember" style="cursor: pointer;">
                Husk mig
              </label>
            </div>
          </div>

          <div v-if="errorMessage" class="error-message text-danger mb-3">
            <i class="bi bi-exclamation-circle-fill me-2"></i>
            <span class="text-red">{{ errorMessage }}</span>
          </div>

          <button type="submit" class="btn-submit" :disabled="loading">
            <span v-if="loading" class="spinner-border spinner-border-sm me-2"></span>
            {{ loading ? 'Logger ind...' : 'Log ind' }}
          </button>

        </form>
      </div>
    </div>
  </div>
</template>

<style scoped>
.login-card {
  max-width: 450px;
  width: 100%;
  border: none;
  border-radius: 12px;
}

.floating-group {
  position: relative;
  width: 100%;
  margin-bottom: 25px;
}

.floating-group input {
  width: 100%;
  padding: 16px 12px 6px 12px;
  border: 1px solid #ccc;
  border-radius: 8px;
  font-size: 1rem;
  transition: border-color 0.3s ease, box-shadow 0.15s ease;
  background: transparent;
  height: 55px;
}

.password-input {
  padding-right: 45px !important;
}

.floating-group label {
  position: absolute;
  left: 12px;
  top: 16px;
  color: #999;
  pointer-events: none;
  transition: 0.2s ease all;
  background-color: white;
  padding: 0 4px;
}

.floating-group input:focus+label,
.floating-group.filled label {
  top: -8px;
  left: 10px;
  font-size: 0.75rem;
  color: #212529;
  font-weight: 600;
}

.floating-group input:focus {
  outline: none;
  border-color: #212529;
  box-shadow: 0 0 0 3px rgba(33, 37, 41, 0.1);
}


.password-toggle {
  position: absolute;
  right: 15px;
  top: 50%;
  transform: translateY(-50%);
  cursor: pointer;
  color: #6c757d;
  font-size: 1.2rem;
  transition: color 0.2s;
  z-index: 10;
}

.password-toggle:hover {
  color: #212529;
}

.btn-submit {
  width: 100%;
  padding: 0.85rem;
  border-radius: 8px;
  font-weight: 600;
  background-color: #212529;
  color: white;
  transition: all 0.2s;
  border: none;
  letter-spacing: 0.5px;
}

.btn-submit:hover:not(:disabled) {
  background-color: #000;
  transform: translateY(-1px);
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}

.btn-submit:disabled {
  background-color: #6c757d;
  cursor: not-allowed;
}

/* [NYT] Error State Styling */

/* Gør kanten rød */
.floating-group.has-error input {
  border-color: #dc3545;
  /* Rød fejlfarve */
  background-color: #fff8f8;
  /* Meget svag rød baggrund (valgfrit) */
}

/* Gør label teksten rød */
.floating-group.has-error label {
  color: #dc3545;
}

/* Gør fokus-effekten rød også, hvis man ikke fjerner klassen (men det gør vi med @focus) */
.floating-group.has-error input:focus {
  box-shadow: 0 0 0 3px rgba(220, 53, 69, 0.25);
}

/* En lille ryste-animation ved fejl (valgfrit, men ser fedt ud) */
.floating-group.has-error {
  animation: shake 0.4s cubic-bezier(.36, .07, .19, .97) both;
}

@keyframes shake {

  10%,
  90% {
    transform: translate3d(-1px, 0, 0);
  }

  20%,
  80% {
    transform: translate3d(2px, 0, 0);
  }

  30%,
  50%,
  70% {
    transform: translate3d(-4px, 0, 0);
  }

  40%,
  60% {
    transform: translate3d(4px, 0, 0);
  }
}
</style>