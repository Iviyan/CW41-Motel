<template>

	<form @submit.prevent="edit">
		<div class="form-floating mb-3">
			<select class="form-select" v-model="serviceOrder.serviceId">
				<option v-for="service in services" :value="service.id">
					{{ service.name }} ({{ service.price }} руб.)
				</option>
			</select>
			<label>Номер</label>
		</div>
		<div class="form-floating mb-3">
			<select class="form-select" v-model="serviceOrder.serviceOrderTypeId">
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

		<input type="submit" class="btn btn-outline-primary" value="Изменить" :disabled="services.length === 0 || rooms.length === 0">
		<router-link :to="{ name: 'service-orders' }" class="btn btn-outline-secondary ms-2">Назад</router-link>
	</form>

	<p class="error-message">{{ error }}</p>

</template>

<script setup>
import {inject, reactive, ref, toRaw, watch} from "vue";
import {RequestError} from "@/exceptions";
import {call_patch} from "@/utils/api";
import router from "@/router";
import {DateTime} from "luxon";

const props = defineProps({
	id: Number
});

const serviceOrders = inject('serviceOrders')
const services = inject('services')
const rooms = inject('rooms')

const serviceOrder = reactive({
	serviceId: services[0]?.id,
	roomNumber: rooms[0]?.number,
	datetime: new Date().toISOString().substring(0, 16)
});

let serviceOrderIndex = -1;

const error = ref('');

async function edit() {
	try {
		let patchData = structuredClone(toRaw(serviceOrder));
		patchData.datetime = DateTime.fromISO(patchData.datetime).toUTC().toISO()

		await call_patch(`/api/service-orders/${props.id}`, patchData);

		Object.assign(serviceOrders.value[serviceOrderIndex], patchData);
		serviceOrders.value[serviceOrderIndex].service = services.value.find(r => r.id === serviceOrder.serviceId)
		error.value = '';
		await router.push({ name: 'service-orders' })
	} catch (err) {
		if (err instanceof RequestError)
			error.value = err.rfc7807 ? err.message : 'Ошибка запроса';
		else
			error.value = 'Неизвестная ошибка';
	}
}

watch(() => props.id, async id => {
	serviceOrderIndex = serviceOrders.value.findIndex(u => u.id === id);
	Object.assign(serviceOrder, serviceOrders.value[serviceOrderIndex]);
	serviceOrder.datetime = DateTime.fromISO(serviceOrder.datetime).toISO().substring(0, 16)
}, { immediate: true })
</script>
