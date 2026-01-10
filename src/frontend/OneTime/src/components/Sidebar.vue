<script setup lang="ts">
import { useAuthStore } from '@/stores/AuthStore';
import { useUserStore } from '@/stores/UserStore';
const AuthStore = useAuthStore()
const userStore = useUserStore()
const role = AuthStore.userRole

const logout = () => {
  AuthStore.logout()
}

</script>

<template>
  <div class="sidebar d-flex flex-column p-4 d-none d-lg-block">
    <router-link to="/" class="d-flex align-items-center mb-5 text-decoration-none">
      <div
        class="rounded-3 text-white d-flex align-items-center justify-content-center me-2"
        style="width: 40px; height: 40px; background-color: var(--primary-color);"
      >
        <i class="bi bi-building-fill fs-5"></i>
      </div>
      <span class="fs-4 fw-bold text-dark" style="letter-spacing: -0.5px">JN-Onetime</span>
    </router-link>

    <template v-if="role === 0" class="nav nav-pills flex-column mb-auto gap-1">
        <router-link to="/admin/users" class="nav-link" active-class="active"> <i class="bi bi-people-fill"></i> Medarbejdere </router-link>
        <router-link to="/admin/projects" class="nav-link" active-class="active"> <i class="bi bi-file-earmark-text"></i> Projekter </router-link>
        <router-link to="/admin/logs" class="nav-link" active-class="active"> <i class="bi bi-terminal"></i> Logs </router-link>
    </template>

    <template v-if="role === 1" class="nav nav-pills flex-column mb-auto gap-1">
      <router-link to="/manager/team-timesheets" class="nav-link" active-class="active"> <i class="bi bi-people-fill"></i> Team Oversigt </router-link>
      <router-link to="/manager/projects" class="nav-link" exact-active-class="active"> <i class="bi bi-file-earmark-text"></i> Projekter </router-link>
      <router-link to="/manager/project-overview" class="nav-link" exact-active-class="active"> <i class="bi bi-bar-chart-line"></i> Team Performance </router-link>
    </template>

    <template v-if="role === 2" class="nav nav-pills flex-column mb-auto gap-1">
      <router-link to="/employee/register" class="nav-link" exact-active-class="active"> <i class="bi bi-calendar-plus"></i> Registrer tid </router-link>
      <router-link to="/employee/stats" class="nav-link" exact-active-class="active"><i class="bi bi-bar-chart-line"></i> Min Statistik</router-link>
    </template>

    <div class="mt-auto pt-4 border-top">
      <div class="d-flex align-items-center gap-3">
        <div class="avatar" :style="'width: 40px; height: 40px; background-color: ' + userStore.getAvatarColor(AuthStore.user!.name)">
          {{ AuthStore.user?.name.charAt(0) }}
        </div>
        <div style="line-height: 1.2">
          <div class="fw-bold text-dark">{{ AuthStore.user?.name }}</div>
          <small class="text-muted">{{ AuthStore.roleText }}</small>
        </div>
      </div>
    </div>
    <button
      class="btn btn-outline-danger mt-4"
      aria-label="Logout"
      @click="logout"
    >Log ud</button>
  </div>
</template>

<style scoped>
.sidebar {
  background: white;
  min-height: 100vh;
  border-right: 1px solid #e2e8f0;
  width: 280px;
}

.nav-link {
  color: #64748b;
  font-weight: 500;
  padding: 12px 20px;
  border-radius: 8px;
  margin-bottom: 5px;
  transition: all 0.2s;
}

.nav-link:hover,
.nav-link.active {
  background-color: var(--primary-light);
  color: var(--primary-color);
}

.nav-link.active {
  font-weight: 600;
}
.nav-link i {
  margin-right: 10px;
}
</style>
