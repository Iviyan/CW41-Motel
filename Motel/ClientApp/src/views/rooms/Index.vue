<template>
	<loader :show="isLoading" />

	<router-view v-if="!isLoading" />
</template>

<script setup>
import {ref, onMounted, provide} from 'vue';
import { useRouter } from 'vue-router';

import Loader from "@/components/Loader";

import { call_get } from '@/utils/api';

const router = useRouter();

const rooms = ref([]);
const roomTypes = ref([]);
provide('rooms', rooms);
provide('roomTypes', roomTypes);

const isLoading = ref(true);

onMounted(async () => {
	isLoading.value = true;
	rooms.value = await call_get(`/api/rooms`)
	roomTypes.value = await call_get(`/api/room-types`)
	isLoading.value = false;
});
</script>
