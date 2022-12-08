<template>

	<form @submit.prevent="add">
		<div class="form-floating mb-3">
			<select class="form-select" v-model="serviceOrder.serviceId">
				<option v-for="service in services" :value="service.id">
					{{ service.name }} ({{ service.price }} руб.)
				</option>
			</select>
			<label>Услуга</label>
		</div>
		<div class="form-floating mb-3">
			<select class="form-select" v-model="serviceOrder.roomNumber">
				<option v-for="room in rooms" :value="room.number">
					{{ room.number }} ({{ room.roomType.name }})
				</option>
			</select>
			<label>Номер</label>
		</div>
		<div class="form-floating mb-3">
			<input type="datetime-local" class="form-control" v-model="serviceOrder.datetime">
			<label>Дата и время</label>
		</div>

		<input type="submit" class="btn btn-outline-primary" value="Добавить заказ" :disabled="services.length === 0 || rooms.length === 0">
		<router-link :to="{ name: 'service-orders' }" class="btn btn-outline-secondary ms-2">Назад</router-link>
	</form>

	<p class="error-message">{{ error }}</p>

</template>

<script setup>
import {inject, provide, reactive, ref} from "vue";
import {RequestError} from "@/exceptions";
import {call_post} from "@/utils/api";
import router from "@/router";
import {DateTime} from "luxon";

const serviceOrders = inject('serviceOrders')
const services = inject('services')
const rooms = inject('rooms')

const serviceOrder = reactive({
	serviceId: services[0]?.id,
	roomNumber: rooms[0]?.number,
	datetime: new Date().toISOString().substring(0, 16)
});

const error = ref('');

async function add() {
	try {
		let data = {
			...serviceOrder,
			datetime: DateTime.fromISO(serviceOrder.datetime).toUTC().toISO(),
		}

		let newServiceOrder = await call_post('/api/service-orders', data)
		newServiceOrder.service = services.value.find(r => r.id === serviceOrder.serviceId)
		serviceOrders.value.push(newServiceOrder);
		error.value = '';
		await router.push({ name: 'service-orders' })
	} catch (err) {
		if (err instanceof RequestError)
			error.value = err.rfc7807 ? err.message : 'Ошибка запроса';
		else
			error.value = 'Неизвестная ошибка';
	}
}

</script>
