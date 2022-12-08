<template>

	<form @submit.prevent="add">
		<div class="form-floating mb-3">
			<input type="text" class="form-control" v-model="leaseAgreement.clientName">
			<label>ФИО клиента</label>
		</div>
		<div class="form-floating mb-3">
			<input type="datetime-local" class="form-control" v-model="leaseAgreement.startAt">
			<label>Дата начала</label>
		</div>
		<div class="form-floating mb-3">
			<input type="datetime-local" class="form-control" v-model="leaseAgreement.endAt">
			<label>Дата конца</label>
		</div>
		<div class="rooms">
			<div class="room" v-for="(room, index) in leaseAgreement.rooms">
				<button type="button" class="btn btn-outline-danger me-2" @click="leaseAgreement.rooms.splice(index, 1)">
					<span class="me-2">{{ room }}</span>
					<svg fill="currentColor" width="16" height="16"><use href="#icon-trash" /></svg>
				</button>
			</div>
		</div>
		<div class="row mb-3 mt-2 gx-3">
			<div class="col">
				<div class="form-floating">
					<select class="form-select" v-model="roomToPush">
						<option v-for="room in roomsToPush" :value="room.number">
							{{ room.number }} ({{ room.roomType.name }})
						</option>
					</select>
					<label>Номер</label>
				</div>
			</div>

			<button type="button" class="btn btn-outline-danger col"
					@click="leaseAgreement.rooms.push(roomToPush)"
					:disabled="!roomToPush"
			>Добавить</button>
		</div>
		<input type="submit" class="btn btn-outline-primary" value="Добавить договор аренды" :disabled="leaseAgreement.rooms.length === 0">
		<router-link :to="{ name: 'lease-agreements' }" class="btn btn-outline-secondary ms-2">Назад</router-link>
	</form>

	<p class="error-message">{{ error }}</p>

</template>

<script setup>
import {computed, inject, provide, reactive, ref, watch} from "vue";
import {RequestError} from "@/exceptions";
import {call_post} from "@/utils/api";
import router from "@/router";
import {DateTime} from "luxon";

const leaseAgreements = inject('leaseAgreements')
const rooms = inject('rooms')

const leaseAgreement = reactive({
	clientName: '',
	startAt: new Date().toISOString().substring(0, 16),
	endAt: new Date().toISOString().substring(0, 16),
	rooms: []
});

const roomsToPush = computed(() => rooms.value.filter(room => !leaseAgreement.rooms.find(r => room.number === r) ))
const roomToPush = ref(null)
watch(roomsToPush, () => {
	let r = roomsToPush.value
	roomToPush.value = r.length === 0 ? null : r[0].number
}, { immediate: true })

const error = ref('');

async function add() {
	try {
		let data = {
			...leaseAgreement,
			startAt: DateTime.fromISO(leaseAgreement.startAt).toUTC().toISO(),
			endAt: DateTime.fromISO(leaseAgreement.endAt).toUTC().toISO()
		}
		let newLeaseAgreement = await call_post('/api/lease-agreements', data)
		leaseAgreements.value.push(newLeaseAgreement);
		error.value = '';
		await router.push({ name: 'lease-agreements' })
	} catch (err) {
		if (err instanceof RequestError)
			error.value = err.rfc7807 ? err.message : 'Ошибка запроса';
		else
			error.value = 'Неизвестная ошибка';
	}
}

</script>

<style scoped>
.rooms {

}
.rooms > .room {
	display: inline-block;
}
</style>
