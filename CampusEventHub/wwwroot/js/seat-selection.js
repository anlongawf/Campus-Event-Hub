// Seat Selection Module - 2D & 3D View
(function() {
    'use strict';

    let selectedSeat = null;
    let scene, camera, renderer, raycaster, mouse, seats = [];
    let viewMode = '3d'; // '3d' or '2d'

    // Initialize when document is ready
    $(document).ready(function() {
        initViewToggle();
        init3DView();
        init2DView();
        initBookButton();
    });

    // Toggle between 2D and 3D views
    function initViewToggle() {
        $('#btn-view-3d').click(function() {
            viewMode = '3d';
            $('#seat-map-3d').show();
            $('#seat-map-2d').hide();
            $(this).addClass('active');
            $('#btn-view-2d').removeClass('active');
            resetSelection();
        });

        $('#btn-view-2d').click(function() {
            viewMode = '2d';
            $('#seat-map-3d').hide();
            $('#seat-map-2d').show();
            $(this).addClass('active');
            $('#btn-view-3d').removeClass('active');
            resetSelection();
        });
    }

    // Reset seat selection
    function resetSelection() {
        if (selectedSeat) {
            if (viewMode === '3d') {
                const prevSeat = seats.find(s => s.userData.seatId === selectedSeat.id);
                if (prevSeat) {
                    prevSeat.traverse((child) => {
                        if (child instanceof THREE.Mesh) {
                            child.material.color.setHex(prevSeat.userData.originalColor);
                        }
                    });
                }
            } else {
                $('.seat-2d.seat-selected-2d').removeClass('seat-selected-2d');
            }
        }
        selectedSeat = null;
        updateSeatInfo();
        $('#btn-book-seat').prop('disabled', true);
    }

    // Update seat information display
    function updateSeatInfo(seat) {
        if (!seat) {
            $('#selected-seat-info').html('<p class="text-muted">Vui lòng chọn ghế</p>');
            return;
        }

        const sectionName = seat.section === 'left' ? 'Trái' : seat.section === 'right' ? 'Phải' : 'Giữa';
        $('#selected-seat-info').html(`
            <div class="alert alert-info mb-0">
                <strong>Ghế đã chọn:</strong><br>
                <strong>Hàng ${seat.row}</strong> - <strong>Số ${seat.number}</strong><br>
                <hr>
                <small><strong>Khu vực:</strong> ${sectionName}</small><br>
                <small><strong>Lối đi gần nhất:</strong> ${seat.nearestAisle || 'Lối đi trung tâm'}</small><br>
                <small><strong>Cửa vào:</strong> ${seat.nearestEntrance || 'Cửa vào chính'}</small>
            </div>
        `);
    }

    // ==================== 3D VIEW ====================
    function init3DView() {
        initThreeJS();
        document.getElementById('seatCanvas').addEventListener('click', onMouseClick);
        window.addEventListener('resize', onWindowResize);
    }

    function initThreeJS() {
        const canvas = document.getElementById('seatCanvas');
        scene = new THREE.Scene();
        scene.background = new THREE.Color(0xf0f0f0);

        camera = new THREE.PerspectiveCamera(60, canvas.clientWidth / canvas.clientHeight, 0.1, 1000);
        renderer = new THREE.WebGLRenderer({ canvas: canvas, antialias: true });
        renderer.setSize(canvas.clientWidth, canvas.clientHeight);
        renderer.shadowMap.enabled = true;
        renderer.shadowMap.type = THREE.PCFSoftShadowMap;

        setupCameraControls();
        camera.position.set(0, 20, 30);
        camera.lookAt(0, 0, 0);

        addLighting();
        addFloor();
        createStage();
        createTheaterStructure();
        createAislesAndEntrances();
        createSeats();
        addSeatLabels();

        raycaster = new THREE.Raycaster();
        mouse = new THREE.Vector2();

        animate();
    }

    function addLighting() {
        const ambientLight = new THREE.AmbientLight(0xffffff, 0.6);
        scene.add(ambientLight);

        const directionalLight = new THREE.DirectionalLight(0xffffff, 0.8);
        directionalLight.position.set(10, 20, 10);
        directionalLight.castShadow = true;
        scene.add(directionalLight);

        const spotLight = new THREE.SpotLight(0xffffff, 0.5);
        spotLight.position.set(0, 15, -10);
        spotLight.target.position.set(0, 0, -8);
        scene.add(spotLight);
        scene.add(spotLight.target);
    }

    function addFloor() {
        const floorGeometry = new THREE.PlaneGeometry(60, 50);
        const floorMaterial = new THREE.MeshStandardMaterial({
            color: 0x2c3e50,
            roughness: 0.8,
            metalness: 0.2
        });
        const floor = new THREE.Mesh(floorGeometry, floorMaterial);
        floor.rotation.x = -Math.PI / 2;
        floor.position.y = -0.5;
        floor.receiveShadow = true;
        scene.add(floor);
    }

    function createStage() {
        const stageGeometry = new THREE.BoxGeometry(35, 1.5, 8);
        const stageMaterial = new THREE.MeshStandardMaterial({
            color: 0x1a1a1a,
            roughness: 0.7,
            metalness: 0.3
        });
        const stage = new THREE.Mesh(stageGeometry, stageMaterial);
        stage.position.set(0, 0.25, -15);
        stage.castShadow = true;
        stage.receiveShadow = true;
        scene.add(stage);

        const curtainGeometry = new THREE.PlaneGeometry(36, 10);
        const curtainMaterial = new THREE.MeshStandardMaterial({
            color: 0x8b0000,
            side: THREE.DoubleSide
        });
        const curtain = new THREE.Mesh(curtainGeometry, curtainMaterial);
        curtain.position.set(0, 5, -19);
        scene.add(curtain);

        const edgeGeometry = new THREE.BoxGeometry(35, 0.3, 0.8);
        const edgeMaterial = new THREE.MeshStandardMaterial({ color: 0xffd700 });
        const edge = new THREE.Mesh(edgeGeometry, edgeMaterial);
        edge.position.set(0, 1, -11);
        scene.add(edge);

        createTextLabel('SÂN KHẤU / STAGE', 0, 1.5, -11, 0xffd700);
    }

    function createTheaterStructure() {
        const columnMaterial = new THREE.MeshStandardMaterial({
            color: 0x34495e,
            roughness: 0.6,
            metalness: 0.4
        });

        const columnPositions = [
            [-22, 0, -10], [22, 0, -10],
            [-22, 0, 0], [22, 0, 0],
            [-22, 0, 10], [22, 0, 10]
        ];

        columnPositions.forEach(pos => {
            const columnGeometry = new THREE.CylinderGeometry(0.8, 1, 12, 8);
            const column = new THREE.Mesh(columnGeometry, columnMaterial);
            column.position.set(pos[0], 6, pos[2]);
            column.castShadow = true;
            scene.add(column);
        });
    }

    function createAislesAndEntrances() {
        const aisleMaterial = new THREE.MeshStandardMaterial({
            color: 0x7f8c8d,
            roughness: 0.9
        });

        const centerAisle = new THREE.BoxGeometry(3, 0.1, 30);
        const centerAisleMesh = new THREE.Mesh(centerAisle, aisleMaterial);
        centerAisleMesh.position.set(0, -0.4, 0);
        scene.add(centerAisleMesh);

        const sideAisle1 = new THREE.BoxGeometry(2, 0.1, 30);
        const sideAisleMesh1 = new THREE.Mesh(sideAisle1, aisleMaterial);
        sideAisleMesh1.position.set(-12, -0.4, 0);
        scene.add(sideAisleMesh1);

        const sideAisleMesh2 = new THREE.Mesh(sideAisle1, aisleMaterial);
        sideAisleMesh2.position.set(12, -0.4, 0);
        scene.add(sideAisleMesh2);

        const entranceMaterial = new THREE.MeshStandardMaterial({
            color: 0x2ecc71,
            emissive: 0x27ae60,
            emissiveIntensity: 0.5
        });

        const entranceGeometry = new THREE.BoxGeometry(3, 2, 0.5);
        const leftEntrance = new THREE.Mesh(entranceGeometry, entranceMaterial);
        leftEntrance.position.set(-18, 1, 15);
        scene.add(leftEntrance);
        createTextLabel('ENTRANCE 1', -18, 2.5, 15, 0x2ecc71);

        const centerEntrance = new THREE.Mesh(entranceGeometry, entranceMaterial);
        centerEntrance.position.set(0, 1, 15);
        scene.add(centerEntrance);
        createTextLabel('ENTRANCE 2', 0, 2.5, 15, 0x2ecc71);

        const rightEntrance = new THREE.Mesh(entranceGeometry, entranceMaterial);
        rightEntrance.position.set(18, 1, 15);
        scene.add(rightEntrance);
        createTextLabel('ENTRANCE 3', 18, 2.5, 15, 0x2ecc71);

        const exitMaterial = new THREE.MeshStandardMaterial({
            color: 0xe74c3c,
            emissive: 0xc0392b,
            emissiveIntensity: 0.5
        });

        const exitGeometry = new THREE.BoxGeometry(2, 2, 0.5);
        const leftExit = new THREE.Mesh(exitGeometry, exitMaterial);
        leftExit.position.set(-22, 1, -5);
        leftExit.rotation.y = Math.PI / 2;
        scene.add(leftExit);
        createTextLabel('EXIT', -23, 2.5, -5, 0xe74c3c);

        const rightExit = new THREE.Mesh(exitGeometry, exitMaterial);
        rightExit.position.set(22, 1, -5);
        rightExit.rotation.y = -Math.PI / 2;
        scene.add(rightExit);
        createTextLabel('EXIT', 23, 2.5, -5, 0xe74c3c);
    }

    function createTextLabel(text, x, y, z, color) {
        const canvas = document.createElement('canvas');
        const context = canvas.getContext('2d');
        canvas.width = 512;
        canvas.height = 128;

        context.fillStyle = 'rgba(0, 0, 0, 0.7)';
        context.fillRect(0, 0, canvas.width, canvas.height);

        context.font = 'Bold 48px Arial';
        context.fillStyle = '#' + color.toString(16).padStart(6, '0');
        context.textAlign = 'center';
        context.textBaseline = 'middle';
        context.fillText(text, canvas.width / 2, canvas.height / 2);

        const texture = new THREE.CanvasTexture(canvas);
        const spriteMaterial = new THREE.SpriteMaterial({ map: texture });
        const sprite = new THREE.Sprite(spriteMaterial);
        sprite.position.set(x, y, z);
        sprite.scale.set(4, 1, 1);
        scene.add(sprite);
    }

    function createSeatGeometry() {
        const group = new THREE.Group();

        const seatBase = new THREE.BoxGeometry(0.7, 0.15, 0.7);
        const seatBaseMesh = new THREE.Mesh(seatBase);
        seatBaseMesh.position.y = 0.35;
        group.add(seatBaseMesh);

        const seatBack = new THREE.BoxGeometry(0.7, 0.7, 0.12);
        const seatBackMesh = new THREE.Mesh(seatBack);
        seatBackMesh.position.set(0, 0.7, 0.35);
        seatBackMesh.rotation.x = -0.15;
        group.add(seatBackMesh);

        const armrestGeometry = new THREE.BoxGeometry(0.1, 0.35, 0.5);
        const leftArmrest = new THREE.Mesh(armrestGeometry);
        leftArmrest.position.set(-0.4, 0.45, 0.1);
        group.add(leftArmrest);

        const rightArmrest = new THREE.Mesh(armrestGeometry);
        rightArmrest.position.set(0.4, 0.45, 0.1);
        group.add(rightArmrest);

        const legGeometry = new THREE.CylinderGeometry(0.05, 0.05, 0.3, 8);
        const legPositions = [
            [-0.3, 0.15, -0.2],
            [0.3, 0.15, -0.2],
            [-0.3, 0.15, 0.3],
            [0.3, 0.15, 0.3]
        ];

        legPositions.forEach(pos => {
            const leg = new THREE.Mesh(legGeometry);
            leg.position.set(pos[0], pos[1], pos[2]);
            group.add(leg);
        });

        return group;
    }

    function createSeats() {
        const seatData = window.seatData;
        const seatsPerRow = Math.max(...seatData.map(s => s.seatNumber));
        const rows = [...new Set(seatData.map(s => s.row))].sort();

        const sections = {
            left: { start: 1, end: Math.floor(seatsPerRow / 3), xOffset: -12 },
            center: { start: Math.floor(seatsPerRow / 3) + 1, end: Math.floor(seatsPerRow * 2 / 3), xOffset: 0 },
            right: { start: Math.floor(seatsPerRow * 2 / 3) + 1, end: seatsPerRow, xOffset: 12 }
        };

        seatData.forEach((seat) => {
            let color;
            switch (seat.status) {
                case 0: color = 0x27ae60; break;
                case 1: color = 0x95a5a6; break;
                case 2: color = 0xe74c3c; break;
                case 3: color = 0x34495e; break;
                default: color = 0x27ae60;
            }

            const seatGroup = createSeatGeometry();
            const material = new THREE.MeshStandardMaterial({
                color: color,
                roughness: 0.7,
                metalness: 0.2
            });

            seatGroup.traverse((child) => {
                if (child instanceof THREE.Mesh) {
                    child.material = material.clone();
                    child.castShadow = true;
                    child.receiveShadow = true;
                }
            });

            let section = 'center';
            let sectionXOffset = 0;
            if (seat.seatNumber <= sections.left.end) {
                section = 'left';
                sectionXOffset = sections.left.xOffset;
            } else if (seat.seatNumber >= sections.right.start) {
                section = 'right';
                sectionXOffset = sections.right.xOffset;
            }

            const rowIndex = seat.row.charCodeAt(0) - 65;
            let localSeatNumber = seat.seatNumber;

            if (section === 'left') {
                localSeatNumber = seat.seatNumber;
            } else if (section === 'center') {
                localSeatNumber = seat.seatNumber - sections.center.start;
            } else {
                localSeatNumber = seat.seatNumber - sections.right.start;
            }

            const xPos = localSeatNumber * 1.2 - 3 + sectionXOffset;
            const zPos = rowIndex * 1.8 - 8;
            const curve = Math.pow(xPos / 15, 2) * 0.3;
            const elevation = rowIndex * 0.15;

            seatGroup.position.set(xPos, curve + elevation, zPos);

            seatGroup.userData = {
                seatId: seat.seatId,
                row: seat.row,
                number: seat.seatNumber,
                status: seat.status,
                section: section,
                originalColor: color,
                nearestAisle: determineNearestAisle(seat.seatNumber, section),
                nearestEntrance: determineNearestEntrance(rowIndex, xPos)
            };

            scene.add(seatGroup);
            seats.push(seatGroup);
        });

        createTextLabel('KHU TRÁI', -12, 0.5, -10, 0x3498db);
        createTextLabel('KHU GIỮA', 0, 0.5, -10, 0x3498db);
        createTextLabel('KHU PHẢI', 12, 0.5, -10, 0x3498db);
    }

    function determineNearestAisle(seatNumber, section) {
        if (section === 'left') return 'Lối đi bên trái';
        if (section === 'right') return 'Lối đi bên phải';
        return 'Lối đi trung tâm';
    }

    function determineNearestEntrance(rowIndex, xPos) {
        if (rowIndex > 10) {
            if (xPos < -10) return 'Cửa vào 1 (Trái)';
            if (xPos > 10) return 'Cửa vào 3 (Phải)';
            return 'Cửa vào 2 (Giữa)';
        }
        return 'Đi qua lối đi chính';
    }

    function addSeatLabels() {
        const rows = ['A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J'];
        rows.forEach((row, index) => {
            const z = index * 1.8 - 8;
            createTextLabel(row, -20, 0.5, z, 0x95a5a6);
            createTextLabel(row, 20, 0.5, z, 0x95a5a6);
        });
    }

    function setupCameraControls() {
        let isDragging = false;
        let previousMousePosition = { x: 0, y: 0 };
        const canvas = document.getElementById('seatCanvas');
        let cameraAngleX = 0;
        let cameraAngleY = 0.6;
        let radius = 35;
        const minRadius = 20;
        const maxRadius = 50;

        function updateCameraPosition() {
            camera.position.x = radius * Math.sin(cameraAngleX) * Math.cos(cameraAngleY);
            camera.position.y = radius * Math.sin(cameraAngleY);
            camera.position.z = radius * Math.cos(cameraAngleX) * Math.cos(cameraAngleY);
            camera.lookAt(0, 0, 0);
        }

        canvas.addEventListener('mousedown', (e) => {
            isDragging = true;
            previousMousePosition = { x: e.clientX, y: e.clientY };
        });

        canvas.addEventListener('mousemove', (e) => {
            if (isDragging) {
                const deltaX = e.clientX - previousMousePosition.x;
                const deltaY = e.clientY - previousMousePosition.y;

                cameraAngleX += deltaX * 0.005;
                cameraAngleY = Math.max(0.1, Math.min(1.5, cameraAngleY - deltaY * 0.005));

                updateCameraPosition();
                previousMousePosition = { x: e.clientX, y: e.clientY };
            }
        });

        canvas.addEventListener('mouseup', () => {
            isDragging = false;
        });

        canvas.addEventListener('mouseleave', () => {
            isDragging = false;
        });

        canvas.addEventListener('wheel', (e) => {
            e.preventDefault();
            const zoomSpeed = 0.1;
            radius += e.deltaY * zoomSpeed;
            radius = Math.max(minRadius, Math.min(maxRadius, radius));
            updateCameraPosition();
        });

        canvas.addEventListener('contextmenu', (e) => e.preventDefault());

        updateCameraPosition();
    }

    function animate() {
        requestAnimationFrame(animate);
        renderer.render(scene, camera);
    }

    function onWindowResize() {
        const canvas = document.getElementById('seatCanvas');
        camera.aspect = canvas.clientWidth / canvas.clientHeight;
        camera.updateProjectionMatrix();
        renderer.setSize(canvas.clientWidth, canvas.clientHeight);
    }

    function onMouseClick(event) {
        event.preventDefault();
        const canvas = document.getElementById('seatCanvas');
        const rect = canvas.getBoundingClientRect();
        mouse.x = ((event.clientX - rect.left) / rect.width) * 2 - 1;
        mouse.y = -((event.clientY - rect.top) / rect.height) * 2 + 1;

        raycaster.setFromCamera(mouse, camera);
        const intersects = raycaster.intersectObjects(seats, true);

        if (intersects.length > 0) {
            let seatGroup = intersects[0].object;
            while (seatGroup.parent && seatGroup.parent.type !== 'Scene') {
                seatGroup = seatGroup.parent;
            }

            if (seatGroup.userData.status !== 0) {
                let statusText = '';
                switch(seatGroup.userData.status) {
                    case 1: statusText = 'Ghế này đã được đặt'; break;
                    case 2: statusText = 'Ghế này đã bị khóa bởi Khoa'; break;
                    case 3: statusText = 'Ghế này không khả dụng'; break;
                }
                alert(statusText);
                return;
            }

            // Reset previous selection
            if (selectedSeat) {
                const prevSeat = seats.find(s => s.userData.seatId === selectedSeat.id);
                if (prevSeat) {
                    prevSeat.traverse((child) => {
                        if (child instanceof THREE.Mesh) {
                            child.material.color.setHex(prevSeat.userData.originalColor);
                        }
                    });
                }
            }

            // Highlight selected seat
            seatGroup.traverse((child) => {
                if (child instanceof THREE.Mesh) {
                    child.material.color.setHex(0xf39c12);
                }
            });

            selectedSeat = {
                id: seatGroup.userData.seatId,
                row: seatGroup.userData.row,
                number: seatGroup.userData.number,
                section: seatGroup.userData.section,
                nearestAisle: seatGroup.userData.nearestAisle,
                nearestEntrance: seatGroup.userData.nearestEntrance
            };

            updateSeatInfo(selectedSeat);
            $('#btn-book-seat').prop('disabled', false);
        }
    }

    // ==================== 2D VIEW ====================
    function init2DView() {
        $('.seat-2d').click(function() {
            const $seat = $(this);
            const seatId = $seat.data('seat-id');
            const row = $seat.data('row');
            const number = $seat.data('number');
            const status = parseInt($seat.data('status'));

            if (status !== 0) {
                let statusText = '';
                switch(status) {
                    case 1: statusText = 'Ghế này đã được đặt'; break;
                    case 2: statusText = 'Ghế này đã bị khóa bởi Khoa'; break;
                    case 3: statusText = 'Ghế này không khả dụng'; break;
                }
                alert(statusText);
                return;
            }

            // Reset previous selection
            $('.seat-2d.seat-selected-2d').removeClass('seat-selected-2d');

            // Highlight selected seat
            $seat.addClass('seat-selected-2d');

            // Determine section based on seat number
            const seatData = window.seatData;
            const seatsPerRow = Math.max(...seatData.map(s => s.seatNumber));
            let section = 'center';
            if (number <= Math.floor(seatsPerRow / 3)) {
                section = 'left';
            } else if (number >= Math.floor(seatsPerRow * 2 / 3) + 1) {
                section = 'right';
            }

            selectedSeat = {
                id: seatId,
                row: row,
                number: number,
                section: section,
                nearestAisle: determineNearestAisle(number, section),
                nearestEntrance: 'Cửa vào chính'
            };

            updateSeatInfo(selectedSeat);
            $('#btn-book-seat').prop('disabled', false);
        });
    }

    // ==================== BOOKING ====================
    function initBookButton() {
        $('#btn-book-seat').click(function() {
            if (!selectedSeat) return;

            const btn = $(this);
            btn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Đang xử lý...');

            $.ajax({
                url: window.bookSeatUrl,
                type: 'POST',
                data: {
                    eventId: window.eventId,
                    seatId: selectedSeat.id
                },
                success: function(response) {
                    if (response.success) {
                        alert('Đặt ghế thành công!\n\nHướng dẫn: ' + selectedSeat.nearestEntrance + '\nĐi theo ' + selectedSeat.nearestAisle);

                        // Update 3D view
                        if (viewMode === '3d') {
                            const seatGroup = seats.find(s => s.userData.seatId === selectedSeat.id);
                            if (seatGroup) {
                                seatGroup.traverse((child) => {
                                    if (child instanceof THREE.Mesh) {
                                        child.material.color.setHex(0x95a5a6);
                                    }
                                });
                                seatGroup.userData.status = 1;
                                seatGroup.userData.originalColor = 0x95a5a6;
                            }
                        } else {
                            // Update 2D view
                            const $seat = $(`.seat-2d[data-seat-id="${selectedSeat.id}"]`);
                            $seat.removeClass('seat-available seat-selected-2d')
                                .addClass('seat-booked')
                                .prop('disabled', true)
                                .attr('data-status', 1);
                        }

                        selectedSeat = null;
                        updateSeatInfo();
                        btn.html('<i class="fas fa-check"></i> Xác nhận đặt ghế');
                    } else {
                        alert('Lỗi: ' + response.message);
                        btn.prop('disabled', false).html('<i class="fas fa-check"></i> Xác nhận đặt ghế');
                    }
                },
                error: function() {
                    alert('Có lỗi xảy ra. Vui lòng thử lại!');
                    btn.prop('disabled', false).html('<i class="fas fa-check"></i> Xác nhận đặt ghế');
                }
            });
        });
    }

})();