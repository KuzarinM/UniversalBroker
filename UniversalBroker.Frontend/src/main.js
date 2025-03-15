import { createApp } from 'vue'
import './style.css'
import App from './App.vue'
import { createMemoryHistory, createWebHistory, createRouter } from 'vue-router'

const routes = [
    { path: '/'},
    // { path: '/about', component: AboutView }
  ]

const router = createRouter({
    //history: createMemoryHistory(),
    history: createWebHistory(),
    routes
  })

createApp(App).use(router).mount('#app')
