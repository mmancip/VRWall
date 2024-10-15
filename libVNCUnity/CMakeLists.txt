cmake_minimum_required (VERSION 3.8)
project(libvncUnity VERSION 0.1.0 LANGUAGES C CXX)

include(CTest)
enable_testing()


file(GLOB_RECURSE Classes
     "src/*.h"
     "src/*.hpp"
     "src/*.cpp"
)

add_subdirectory(libs/libvncserver)
add_library(libvncUnity SHARED ${Classes})
#add_executable(libvncUnity  ${Classes})

include_directories(libs/libvncserver/include)     
target_compile_definitions(libvncUnity PRIVATE VNC_UNITY_EXPORTS)


set(CPACK_PROJECT_NAME ${PROJECT_NAME})
set(CPACK_PROJECT_VERSION ${PROJECT_VERSION})
include(CPack)

target_link_libraries(${CMAKE_PROJECT_NAME} vncclient )