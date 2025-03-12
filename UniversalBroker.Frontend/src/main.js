import { createApp } from 'vue'
import './style.css'
import App from './App.vue'
import { createMemoryHistory, createRouter } from 'vue-router'

const routes = [
    // { path: '/', component: HomeView },
    // { path: '/about', component: AboutView }
  ]

const router = createRouter({
    history: createMemoryHistory(),
    routes
  })

createApp(App).use(router).mount('#app')
