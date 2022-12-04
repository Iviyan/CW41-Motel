import { createRouter, createWebHistory } from 'vue-router'
import store from '@/store'
import roles from '@/utils/roles'

const LoginView = () => import('../views/Login.vue')

const MainView = () => import('../views/Main')
const ChangePasswordView = () => import('../views/ChangePassword')
const HomeView = () => import('../views/Home')

const EmployeesView = () => import('../views/employees/Index')
const EmployeesListView = () => import('../views/employees/List')
const EmployeesNewView = () => import('../views/employees/New')
const EmployeesEditView = () => import('../views/employees/Edit')

const isNotAuthenticated = (to, from) => {
	console.log(store);
	if (store.getters.isAuth)
		return '/';
}

const isAuthenticated = (to, from) => {
	if (!store.getters.isAuth && to.name !== 'Login') {
		return { name: 'login' }
	}
}

const isRole = (...roles) => () => roles.includes(store.getters.role);

const routes = [
	{
		path: '/login',
		name: 'login',
		component: LoginView,
		beforeEnter: isNotAuthenticated,
		meta: { title: 'Вход' }
	},
	{
		path: '/',
		name: 'main',
		component: MainView,
		beforeEnter: isAuthenticated,
		children: [
			{ path: '', name: 'home', component: HomeView, meta: { title: 'Главная' } },
			{
				path: 'employees',
				component: EmployeesView,
				beforeEnter: isRole(roles.admin, roles.hr),
				children: [
					{
						name: 'employees-new',
						path: 'new',
						component: EmployeesNewView
					},
					{
						name: 'employees',
						path: '',
						component: EmployeesListView
					},
					{
						name: 'employees-edit',
						path: ':id(\\d+)',
						component: EmployeesEditView,
						props: route => ({ id: Number(route.params.id) })
					},
				]
			},

			{
				path: 'change-password', name: 'changePassword',
				component: ChangePasswordView, meta: { title: 'Изменение пароля' },
			},
		]
	},
	{ path: '/:catchAll(.*)*', redirect: '/' }
]

const router = createRouter({
	history: createWebHistory(),
	routes
})

router.afterEach((to, from) => {
	if (to.meta.title) document.title = to.meta.title;
})

export default router
