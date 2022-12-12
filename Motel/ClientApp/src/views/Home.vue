<template>
	<h1 style="text-align: center">Мотель</h1>

	<p class="fs-5 ms-3">Адрес: г. Моксва, ул. Пушкина, д.2</p>

	<h3 class="header">Номера</h3>

	<table class="table mt-3 mx-3">
		<thead><tr>
			<th>Номер</th>
			<th>Тип</th>
			<th>Свободен</th>
		</tr></thead>
		<tbody><tr v-for="room in rooms">
			<td>{{ room.number }}</td>
			<td>{{ room.roomType.name }} ({{ room.roomType.pricePerHour }} руб./час, {{ room.roomType.capacity }} чел.)</td>
			<td><input class="form-check-input" type="checkbox" disabled :checked="room.isFree"></td>
		</tr></tbody>
	</table>

	<div class="d-flex mx-3 mt-5 mb-3">
	<router-link class="btn btn-outline-primary ms-auto" :to="{name: 'login'}">Авторизация</router-link>
	</div>
</template>

<script setup>
import {onMounted, provide, ref} from "vue";
import {call_get} from "@/utils/api";
import {getj} from "@/utils/fetch";

const rooms = ref([]);

const isLoading = ref(true);

onMounted(async () => {
	rooms.value = await getj(`/api/rooms/public`)
});
</script>

<style scoped>
.header {
	margin: 0 0 10px 0;
	text-align: center;
	font-weight: 500;
}
</style>
