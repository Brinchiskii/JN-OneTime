<script setup lang="ts">
import { useAuthStore } from '@/stores/AuthStore';
const AuthStore = useAuthStore()

const Role = AuthStore.userRole

const menuItems = [
]



</script>

<template>
  <div class="sidebar d-flex flex-column p-4 d-none d-lg-block">
    <a href="#" class="d-flex align-items-center mb-5 text-decoration-none">
      <div
        class="rounded-3 text-white d-flex align-items-center justify-content-center me-2"
        style="width: 40px; height: 40px; background-color: var(--primary-color)"
      >
        <i class="bi bi-building-fill fs-5"></i>
      </div>
      <span class="fs-4 fw-bold text-dark" style="letter-spacing: -0.5px">TimeSync</span>
    </a>

    <ul class="nav nav-pills flex-column mb-auto gap-1">
      <li class="nav-item">
        <a href="#" class="nav-link"> <i class="bi bi-speedometer2"></i> Dashboard </a>
      </li>
      <li>
        <a href="#" class="nav-link active"> <i class="bi bi-people-fill"></i> Medarbejdere </a>
      </li>
      <li>
        <a href="#" class="nav-link"> <i class="bi bi-file-earmark-text"></i> Rapporter </a>
      </li>
    </ul>

    <div class="mt-auto pt-4 border-top">
      <div class="d-flex align-items-center gap-3">
        <div class="avatar bg-secondary">L</div>
        <div style="line-height: 1.2">
          <div class="fw-bold text-dark">Lars</div>
          <small class="text-muted">Manager</small>
        </div>
      </div>
    </div>
    <router-link
      class="btn btn-outline-danger mt-4"
      to="/login"
      aria-label="Logout"
    >Log ud</router-link>
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
