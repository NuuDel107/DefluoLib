extends Node

var studio_system = FMODStudioModule.get_studio_system()

# Banks

func load_bank(bank_path: String):
	studio_system.load_bank_file(bank_path, FMODStudioModule.FMOD_STUDIO_LOAD_BANK_NORMAL, false)

func get_bank(bank_path: String):
	return studio_system.get_bank(bank_path)

func unload_bank(bank: Bank):
	bank.unload();

# Event assets

func play_one_shot(event: EventAsset):
	FMODRuntime.play_one_shot(event)

func play_one_shot_attached(event: EventAsset, node: Node3D):
	FMODRuntime.play_one_shot(event, node)

func play_one_shot_at_position(event: EventAsset, position: Vector3):
	FMODRuntime.play_one_shot(event, position)

# Event instances

func create_event_instance(event_path: String):
	return FMODRuntime.create_instance_path(event_path)

func update_instance_3d(instance: EventInstance, position: Vector3):
	var attributes = FMOD_3D_ATTRIBUTES.new()
	RuntimeUtils.to_3d_attributes(attributes, position)
	instance.set_3d_attributes(attributes)
	
func start_instance(instance: EventInstance):
	instance.start()

func stop_instance(instance: EventInstance, allow_fadeout: bool):
	if allow_fadeout:
		instance.stop(FMODStudioModule.FMOD_STUDIO_STOP_ALLOWFADEOUT)
	else:
		instance.stop(FMODStudioModule.FMOD_STUDIO_STOP_IMMEDIATE)

func set_instance_paused(instance: EventInstance, paused: bool):
	instance.set_paused(paused)

func release_instance(instance: EventInstance):
	instance.release()

func add_instance_callback(instance: EventInstance, callable: Callable, callback_type: int):
	instance.set_callback(callable, callback_type)

# Buses

func get_bus(bus_path: String):
	return studio_system.get_bus(bus_path)
	
func set_bus_volume(bus: Bus, volume: float):
	bus.set_volume(volume)

func set_bus_paused(bus: Bus, paused: float):
	bus.set_paused(paused)

# VCAs

func get_vca(vca_path: String):
	return studio_system.get_vca(vca_path)

func set_vca_volume(vca: VCA, volume: float):
	vca.set_volume(volume);

# Parameters

func set_instance_float_parameter(instance: EventInstance, parameter_name: String, value: float):
	instance.set_parameter_by_name(parameter_name, value)

func set_instance_label_parameter(instance: EventInstance, parameter_name: String, value: String):
	instance.set_parameter_by_name_with_label(parameter_name, value)

func set_global_float_parameter(parameter_name: String, value: float):
	studio_system.set_parameter_by_name(parameter_name, value)
	
func set_global_label_parameter(parameter_name: String, value: String):
	studio_system.set_parameter_by_name_with_label(parameter_name, value)
