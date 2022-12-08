<template>

	<form @submit.prevent="edit">
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

		<input type="submit" class="btn btn-outline-primary" value="Изменить">
		<router-link :to="{ name: 'lease-agreements' }" class="btn btn-outline-secondary ms-2">Назад</router-link>
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

const leaseAgreements = inject('leaseAgreements')
const rooms = inject('rooms')

const leaseAgreement = reactive({
	clientName: '',
	startAt: new Date().toInputDateTime(),
	endAt: new Date().toInputDateTime(),
});

let leaseAgreementIndex = -1;

const error = ref('');

async function edit() {
	try {
		let patchData = structuredClone(toRaw(leaseAgreement));
		patchData.startAt = DateTime.fromISO(patchData.startAt).toUTC().toISO()
		patchData.endAt = DateTime.fromISO(patchData.endAt).toUTC().toISO()

		await call_patch(`/api/lease-agreements/${props.id}`, patchData);

		Object.assign(leaseAgreements.value[leaseAgreementIndex], patchData);
		error.value = '';
		await router.push({ name: 'lease-agreements' })
	} catch (err) {
		if (err instanceof RequestError)
			error.value = err.rfc7807 ? err.message : 'Ошибка запроса';
		else
			error.value = 'Неизвестная ошибка';
	}
}

watch(() => props.id, async id => {
	leaseAgreementIndex = leaseAgreements.value.findIndex(u => u.id === id);
	Object.assign(leaseAgreement, leaseAgreements.value[leaseAgreementIndex]);
	leaseAgreement.startAt = DateTime.fromISO(leaseAgreement.startAt).toISO().substring(0, 16)
	leaseAgreement.endAt = DateTime.fromISO(leaseAgreement.endAt).toISO().substring(0, 16)
}, { immediate: true })
</script>
