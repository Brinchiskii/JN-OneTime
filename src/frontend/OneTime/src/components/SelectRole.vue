<script setup lang="ts"></script>

<template>
  <div
    class="container d-flex justify-content-center align-items-center vh-100"
  >
    <div class="card select-role-card shadow">
      <div class="card-body text-center p-4">
        <h5 class="card-title">JN OneTime</h5>
        <p class="mb-4">Select your role to continue</p>
        <div class="d-grid gap-3 mx-auto mt-4">
          <router-link
            class="btn btn-outline-dark d-flex flex-column align-items-center py-4 px-5"
            to="/employee"
            aria-label="Employee dashboard"
          >
            <svg
              xmlns="http://www.w3.org/2000/svg"
              width="24"
              height="24"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              stroke-width="2"
              stroke-linecap="round"
              stroke-linejoin="round"
              class="lucide lucide-users h-8 w-8"
              aria-hidden="true"
            >
              <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"></path>
              <path d="M16 3.128a4 4 0 0 1 0 7.744"></path>
              <path d="M22 21v-2a4 4 0 0 0-3-3.87"></path>
              <circle cx="9" cy="7" r="4"></circle>
            </svg>
            <span>Employee</span>
          </router-link>

          <router-link
            class="btn btn-outline-dark d-flex flex-column align-items-center py-4 px-5"
            to="/manager"
            aria-label="Manager dashboard"
          >
            <svg
              xmlns="http://www.w3.org/2000/svg"
              width="24"
              height="24"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              stroke-width="2"
              stroke-linecap="round"
              stroke-linejoin="round"
              class="lucide lucide-user-cog h-8 w-8"
              aria-hidden="true"
            >
              <path d="M10 15H6a4 4 0 0 0-4 4v2"></path>
              <path d="m14.305 16.53.923-.382"></path>
              <path d="m15.228 13.852-.923-.383"></path>
              <path d="m16.852 12.228-.383-.923"></path>
              <path d="m16.852 17.772-.383.924"></path>
              <path d="m19.148 12.228.383-.923"></path>
              <path d="m19.53 18.696-.382-.924"></path>
              <path d="m20.772 13.852.924-.383"></path>
              <path d="m20.772 16.148.924.383"></path>
              <circle cx="18" cy="15" r="3"></circle>
              <circle cx="9" cy="7" r="4"></circle>
            </svg>
            <span>Manager</span>
          </router-link>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.select-role-card {
  max-width: 500px;
  width: 100%;
}
</style>

