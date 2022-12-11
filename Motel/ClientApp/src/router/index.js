import {createRouter, createWebHistory} from 'vue-router'
import store from '@/store'
import roles from '@/utils/roles'

const LoginView = () => import('../views/Login.vue')

const MainView = () => import('../views/Main')
const ChangePasswordView = () => import('../views/ChangePassword')
const HomeView = () => import('../views/Home')
const AdminInfoView = () => import('../views/adminInfo')

const EmployeesView = () => import('../views/employees/Index')
const EmployeesListView = () => import('../views/employees/List')
const EmployeesNewView = () => import('../views/employees/New')
const EmployeesEditView = () => import('../views/employees/Edit')

const RoomTypesView = () => import('../views/roomTypes/Index')
const RoomTypesListView = () => import('../views/roomTypes/List')
const RoomTypesNewView = () => import('../views/roomTypes/New')
const RoomTypesEditView = () => import('../views/roomTypes/Edit')

const ServicesView = () => import('../views/services/Index')
const ServicesListView = () => import('../views/services/List')
const ServicesNewView = () => import('../views/services/New')
const ServicesEditView = () => import('../views/services/Edit')

const RoomsView = () => import('../views/rooms/Index')
const RoomsListView = () => import('../views/rooms/List')
const RoomsNewView = () => import('../views/rooms/New')
const RoomsEditView = () => import('../views/rooms/Edit')

const ServiceOrdersView = () => import('../views/serviceOrders/Index')
const ServiceOrdersListView = () => import('../views/serviceOrders/List')
const ServiceOrdersNewView = () => import('../views/serviceOrders/New')
const ServiceOrdersEditView = () => import('../views/serviceOrders/Edit')

const LeaseAgreementsView = () => import('../views/leaseAgreements/Index')
const LeaseAgreementsListView = () => import('../views/leaseAgreements/List')
const LeaseAgreementsNewView = () => import('../views/leaseAgreements/New')
const LeaseAgreementsEditView = () => import('../views/leaseAgreements/Edit')
const LeaseAgreementsGraphView = () => import('../views/leaseAgreements/Graph')

const AdvertisingContractsView = () => import('../views/advertisingContracts/Index')
const AdvertisingContractsListView = () => import('../views/advertisingContracts/List')
const AdvertisingContractsNewView = () => import('../views/advertisingContracts/New')
const AdvertisingContractsEditView = () => import('../views/advertisingContracts/Edit')

const RoomCleaningsView = () => import('../views/roomCleanings/Index')
const RoomCleaningsListView = () => import('../views/roomCleanings/List')

const isNotAuthenticated = (to, from) => {
	console.log(store);
	if (store.getters.isAuth)
		return '/';
}

const isAuthenticated = (to, from) => {
	if (!store.getters.isAuth && to.name !== 'Login') {
		return {name: 'login'}
	}
}

const isRole = (...roles) => () => roles.includes(store.getters.role);

const routes = [
	{
		path: '/login',
		name: 'login',
		component: LoginView,
		beforeEnter: isNotAuthenticated,
		meta: {title: 'Вход'}
	},
	{
		path: '/',
		name: 'main',
		component: MainView,
		beforeEnter: isAuthenticated,
		children: [
			{path: '', name: 'home', component: HomeView, meta: {title: 'Главная'}},
			{
				path: 'employees',
				component: EmployeesView,
				beforeEnter: isRole(roles.admin, roles.hr),
				children: [
					{name: 'employees-new', path: 'new', component: EmployeesNewView},
					{name: 'employees', path: '', component: EmployeesListView},
					{
						name: 'employees-edit', path: ':id(\\d+)', component: EmployeesEditView,
						props: route => ({id: Number(route.params.id)})
					},
				]
			},
			{
				path: 'room-types',
				component: RoomTypesView,
				beforeEnter: isRole(roles.admin, roles.salesman),
				children: [
					{name: 'room-types-new', path: 'new', component: RoomTypesNewView},
					{name: 'room-types', path: '', component: RoomTypesListView},
					{
						name: 'room-types-edit', path: ':id(\\d+)', component: RoomTypesEditView,
						props: route => ({id: Number(route.params.id)})
					},
				]
			},
			{
				path: 'services',
				component: ServicesView,
				beforeEnter: isRole(roles.admin, roles.salesman),
				children: [
					{name: 'services-new', path: 'new', component: ServicesNewView},
					{name: 'services', path: '', component: ServicesListView},
					{
						name: 'services-edit', path: ':id(\\d+)', component: ServicesEditView,
						props: route => ({id: Number(route.params.id)})
					},
				]
			},
			{
				path: 'rooms',
				component: RoomsView,
				beforeEnter: isRole(roles.admin, roles.salesman, roles.maid),
				children: [
					{name: 'rooms-new', path: 'new', component: RoomsNewView},
					{name: 'rooms', path: '', component: RoomsListView},
					{
						name: 'rooms-edit', path: ':id(\\d+)', component: RoomsEditView,
						props: route => ({id: Number(route.params.id)})
					},
				]
			},
			{
				path: 'room-cleanings',
				component: RoomCleaningsView,
				beforeEnter: isRole(roles.admin, roles.salesman, roles.maid),
				children: [
					{name: 'room-cleanings', path: '', component: RoomCleaningsListView},
				]
			},
			{
				path: 'service-orders',
				component: ServiceOrdersView,
				beforeEnter: isRole(roles.admin, roles.salesman),
				children: [
					{name: 'service-orders-new', path: 'new', component: ServiceOrdersNewView},
					{name: 'service-orders', path: '', component: ServiceOrdersListView},
					{
						name: 'service-orders-edit', path: ':id(\\d+)', component: ServiceOrdersEditView,
						props: route => ({id: Number(route.params.id)})
					},
				]
			},
			{
				path: 'lease-agreements',
				component: LeaseAgreementsView,
				beforeEnter: isRole(roles.admin, roles.salesman),
				children: [
					{name: 'lease-agreements-new', path: 'new', component: LeaseAgreementsNewView},
					{name: 'lease-agreements', path: '', component: LeaseAgreementsListView},
					{
						name: 'lease-agreements-edit', path: ':id(\\d+)', component: LeaseAgreementsEditView,
						props: route => ({id: Number(route.params.id)})
					},
					{name: 'lease-agreements-graph', path: 'graph', component: LeaseAgreementsGraphView},
				]
			},
			{
				path: 'advertising-contracts',
				component: AdvertisingContractsView,
				beforeEnter: isRole(roles.admin, roles.marketingSpecialist),
				children: [
					{name: 'advertising-contracts-new', path: 'new', component: AdvertisingContractsNewView},
					{name: 'advertising-contracts', path: '', component: AdvertisingContractsListView},
					{
						name: 'advertising-contracts-edit', path: ':id(\\d+)', component: AdvertisingContractsEditView,
						props: route => ({id: Number(route.params.id)})
					}
				]
			},
			{
				path: 'admin-info', name: 'admin-info',
				beforeEnter: isRole(roles.admin),
				component: AdminInfoView,
			},
			{
				path: 'change-password', name: 'changePassword',
				component: ChangePasswordView, meta: {title: 'Изменение пароля'},
			},
		]
	},
	{path: '/:catchAll(.*)*', redirect: '/'}
]

const router = createRouter({
	history: createWebHistory(),
	routes
})

router.afterEach((to, from) => {
	if (to.meta.title) document.title = to.meta.title;
})

export default router
