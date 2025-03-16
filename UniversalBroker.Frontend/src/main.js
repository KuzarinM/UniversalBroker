import { createApp } from 'vue'
import './style.css'
import App from './App.vue'
import { createMemoryHistory, createWebHistory, createRouter } from 'vue-router'
import ConnectionsPage from './pages/ConnectionsPage.vue'
import CommunicationsPage from './pages/CommunicationsPage.vue'
import ChanelsPage from './pages/ChanelsPage.vue'


const routes = [
    { path: '/', component: CommunicationsPage},
    { path: '/connections', component: ConnectionsPage},
    { path: '/chanels', component: ChanelsPage},
    // { path: '/about', component: AboutView }
  ]

const router = createRouter({
    //history: createMemoryHistory(),
    history: createWebHistory(),
    routes
  })

createApp(App).use(router).mount('#app')
