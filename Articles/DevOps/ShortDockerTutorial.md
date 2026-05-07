# Docker tutorial - building and running
*Author: Kasper B. Graversen*
<ArticleHeaderUrls/><Categories Tags="Docker, CI/CD">
</Categories>

Here is a small tutorial for getting started with Docker. The key difference between this tutorial and many others is that all unnecessary complexity has been shaved away.
We introduce the concepts in docker gradually as we need them. 


Table of contents

   * [Images and containers](#images-and-containers)
   * [Creating our first image](#creating-our-first-image)
   * [Build the image](#build-the-image)
   * [See the result](#see-the-result)
   * [Save the image to a file on the filesystem](#save-the-image-to-a-file-on-the-filesystem)
   * [Run the image as a container](#run-the-image-as-a-container)
   * ['CMD' - automatically run a command](#cmd---automatically-run-a-command)
   * ['ENTRYPOINT' - define the main command](#entrypoint---define-the-main-command)
   * ['COPY' / 'ADD' - add artifacts to the image](#copy--add---add-artifacts-to-the-image)
   * [Persist data with a Docker volume](#persist-data-with-a-docker-volume)
   * [Mount a folder from the host machine](#mount-a-folder-from-the-host-machine)
   * [Next steps for the tutorial](#next-steps-for-the-tutorial)


## Images and containers
In programming, we have classes and objects that are instances of classes. In the Docker world, an image is comparable to a class, and a container is comparable to an object.

## Creating our first image

Create a file named `Dockerfile` in an empty folder with this content:

```
FROM ubuntu:latest
```


### Build the image

In the same folder, run:

```
docker build -t my-stuff:1.0 -t my-stuff:latest .
```


Flags explained:

* `-t my-stuff:1.0` = a tag, meaning the name and version of your image
* `-t my-stuff:latest` = another tag for the same image
* `.` = build from the Dockerfile in the current folder

We tag the image so we can find it again easily. We use *two* tags, so we have both a versioned tag and a `latest` tag. The `latest` tag makes it easy to refer to the most recent build, similar to how we use `FROM ubuntu:latest`.


### See the result
Looking in the current folder will not show anything new:

```
> dir

Mode                 LastWriteTime         Length Name
----                 -------------         ------ ----
-a---          05-05-2026    08:16             37 dockerfile
```

The image is only visible inside Docker's internal image store:

```
> docker image ls

REPOSITORY                 TAG       IMAGE ID       CREATED        SIZE
my-stuff                   1.0       4e759ff0d3fd   13 days ago    157MB
my-stuff                   latest    4e759ff0d3fd   13 days ago    157MB
...
```


### Save the image to a file on the filesystem
We can save the image to the filesystem. Docker saves images as tar files. Other formats, such as zip files, are not supported by `docker save`.


```
> docker save -o my.tar my-stuff:1.0
> dir

Mode                 LastWriteTime         Length Name
----                 -------------         ------ ----
-a---          05-05-2026    08:16             37 dockerfile
-a---          05-05-2026    08:25       41.574.400 my.tar
```


## Running the image as a container

To create a container from an image and get an interactive shell in Ubuntu Linux, run:

```
> docker run -it my-stuff:1.0

root@2bd442a1e2d1:/#
```

`-it` is important because Bash is interactive. Without it, Bash will stop immediately.

* `-i` keeps input open
* `-t` creates a terminal

We can also run the container with the command `ls` to see the content of the startup folder:

```
> docker run -t my-stuff:1.0 ls

bin  boot  dev  etc  home  lib  lib64  media  mnt  opt  proc  root  run  sbin  srv  sys  tmp  usr  var
```


 

## 'ENTRYPOINT' and 'CMD' - command and argument on startup

`ENTRYPOINT` defines the main executable for the container when it is starting. `CMD` defines the argument that is passed along. 

Change the `Dockerfile` to:

```
FROM ubuntu:latest
ENTRYPOINT [ "echo" ]
CMD [ "Hello World", "again.." ]

> docker build -t my-stuff:latest .
> docker run my-stuff:latest

Hello World again..
```

If we pass a command parameter such as `ls`, as we did earlier, we do not get the folder content.

```
> docker run my-stuff:latest ls

ls
```

This happens because `ls` is passed as an argument to `echo`. The result is the same as running `echo ls`.

When debugging a faulty image, it can be helpful to alternate what is started. The `ENTRYPOINT` can be changed using  `--entrypoint`:

```
> docker run --entrypoint ls my-stuff:latest /

bin
boot
dev
...
```

Only one `CMD` instruction can be used. If the Dockerfile contains multiple `CMD` instructions, only the last one is used.


### When  'CMD' becomes the command

I am not sure why this is possible or even sane. But you may omit `ENTRYPOINT` and only use `CMD`. In this situation,
`CMD` defines the default command, that runs when the container starts. 

Let's modify our `Dockerfile` to:

```
FROM ubuntu:latest
CMD ["sh", "-c", "echo First && echo Second && echo Third"]
```

Build and run the new image:

```
> docker build -t my-stuff:3.0 -t my-stuff:latest .
> docker run my-stuff:latest

First
Second
Third
```

`CMD` can be overridden. We can replace the command at runtime:

```
> docker run -t my-stuff:latest ls

bin  boot  dev  etc  home  lib  lib64  media  mnt  opt  proc  root  run  sbin  srv  sys  tmp  usr  var
```

The `ls` argument replaces the whole `CMD`-definition from the `dockerfiile`. So Docker runs `ls` instead of the default command.


### Warning about []
The `[]` parenthesis enable us to supply more than one argument to `ENTRYPOINT` and `CMD`. But is also changes how the arguement to these are handled. Try removing the `[]` and see how the docker image no longer work. 

```
FROM ubuntu:latest
ENTRYPOINT "echo"
CMD "Hello World"
```
and run

```
> docker build -t my-stuff:latest .
 2 warnings found (use docker --debug to expand):
 - JSONArgsRecommended: JSON arguments recommended for ENTRYPOINT to prevent unintended behavior related to OS signals (line 2)
 - JSONArgsRecommended: JSON arguments recommended for CMD to prevent unintended behavior related to OS signals (line 3)

> docker run my-stuff:latest

(( missing output!))
```

Feel free to dig more into this if you find it interesting. For now, perhaps just remember to always use `[]`


## 'COPY' / 'ADD' - getting artifacts into the image
Now that the basics are in place, we can focus on putting a program into the Docker image. Use `ADD` or `COPY` for this. `ADD` can fetch resources from the internet and unpack local tar files, which makes it less predictable than `COPY`. Prefer `COPY` when possible.

Let's make a small program that prints the numbers 1 to 10. For now, we can imagine that this is a more complex application. By using Bash as the programming language we are free from having to install extra infrastructure (e.g. a Python, Java or C# run-time). Later we will use these languages.

Create a new file named `print_numbers.sh` with this content:

```
for i in $(seq 1 10); do echo "$i"; done
```

If you are on Windows, make sure the file uses Unix-style line endings. Or simply make sure the file is only one line long.

Change the `Dockerfile` to:

```
FROM ubuntu:latest
COPY print_numbers.sh /print_numbers.sh
RUN chmod +x /print_numbers.sh
ENTRYPOINT ["/bin/bash"]
CMD ["/print_numbers.sh"]
```

We copy our application to the `/app/` folder to separate it from the tools we may need to install later. We use `chmod` to make the script executable. Finally, we set it as the default command using `ENTRYPOINT` + `CMD`.

```
> docker build -t my-stuff:latest .
> docker run my-stuff:latest

1
2
...
```


## Persist data inside 
A container is running on the host operating system, while being isolated from it. This means a program inside a container cannot write to the "outside" file system unless given permission. Let us play with storing state locally inside the container and then move on to storing the same state outside.

Change the `dockerfile` to 

```
FROM ubuntu:latest
RUN echo "test data" > /data/file.txt
```

and run

```
> docker build -t my-stuff:latest .
> docker run my-stuff:latest
```

/data/file.txt ligger i container-lagene (overlay filesystem)
Når containeren stoppes, bevares den (exit code 0)
Når du sletter containeren: docker rm, slettes også alle filer


docker commit running-container my-new-image
Det laver en ny image med alle filer fra containeren.


Perhaps you want to run a database as a container. The database will not be able to store its rows outside the container. This hardly  We can define a `VOLUME` inside our `Dockerfile` and then when invoking the image, specify where on the host operating system that points to.

This enable for example, you 





Hvis containeren har kørt færdig (exit code 0), kan du stadig se filerne:

Metode 1: docker diff (se hvad der ændret sig)

docker diff container-name
Output viser alle filer der blev tilføjet/ændret.

Metode 2: docker cp (kopier filer ud)

docker cp container-name:/data/file.txt ./local-file.txt
Kopierer /data/file.txt fra stoppet container til din computer.

Metode 3: docker exec (hvis container stadig kører)

docker exec container-name cat /data/file.txt
Metode 4: Start containeren igen

docker start container-name
docker exec container-name cat /data/file.txt
Eksempel:

FROM ubuntu:latest
RUN echo "test data" > /data/file.txt
docker build -t my-image .
docker run --name my-container my-image
docker cp my-container:/data/file.txt ./my-file.txt
cat ./my-file.txt
Output: test data

OBS: Når du køre docker rm my-container, slettes filerne permanent. Brug docker cp før du sletter!





FROM ubuntu:latest
VOLUME /data
RUN echo "test data" > /data/file.txt
Uden -v:

docker run my-image
/data/file.txt gemmes i en anonym volume
Når containeren stoppes, bevares data i volumenen (ikke slettet)
Men du kan ikke finde volumenen let — den har et tilfældigt ID
Se den:

docker volume ls



## Persist state outside the container



## Persist data with a Docker volume

Normally, files written inside a container disappear when the container is removed. A Docker volume is storage managed by Docker that can live longer than any single container.

First, create a volume:

```
> docker volume create my-volume

my-volume
```

Now start a container, mount the volume at `/data`, and write a file into it:

```
> docker run --rm -v my-volume:/data ubuntu:latest sh -c "echo Hello from the first container > /data/message.txt"
```

The container has now stopped. Because we used `--rm`, the container itself has also been removed. The volume still exists.

Start a new container, mount the same volume, and print the file:

```
> docker run --rm -v my-volume:/data ubuntu:latest cat /data/message.txt

Hello from the first container
```

The file survived because it was written to the volume, not only to the container's own filesystem.

When you are done with the volume, remove it:

```
> docker volume rm my-volume

my-volume
```


## Mount a folder from the host machine

Sometimes you do not want Docker to manage the storage. Instead, you want the container to read and write directly to a normal folder on your machine. This is called a bind mount.

Create a folder on the host machine:

```
> mkdir docker-host-data
```

On Windows PowerShell, run a container that mounts this folder as `/data` inside the container:

```
> docker run --rm -v ${PWD}\docker-host-data:/data ubuntu:latest sh -c "echo Hello from Docker > /data/message.txt"
```

The container writes the file to `/data/message.txt`, but `/data` points to the `docker-host-data` folder on your machine.

If we now look in the folder outside Docker, we can see the file:

```
> dir .\docker-host-data

Mode                 LastWriteTime         Length Name
----                 -------------         ------ ----
-a---          05-05-2026    09:15             18 message.txt
```

And we can print the file from PowerShell:

```
> Get-Content .\docker-host-data\message.txt

Hello from Docker
```

Volumes are useful when Docker should own the storage. Bind mounts are useful when the host machine and the container should share a specific folder.

## RUN - execute during build time
RUN echo "test data" > /data/file.txt

we use this for downloading dependencies ...c# example

## Next steps for the tutorial

The next natural steps are to expand the tutorial from a single-file demo into the workflow used for real applications:

1. Clean up local Docker resources
   * Show `docker ps`, `docker ps -a`, `docker stop`, `docker rm`, and `docker image rm`.
   * Explain the difference between removing containers and removing images.

2. Inspect containers and images
   * Use `docker inspect` to see metadata, configuration, environment variables, mounts, and network settings.
   * Use `docker logs` to inspect output from a container after it has started.

3. Run containers in the background
   * Introduce detached mode with `docker run -d`.
   * Show how to follow logs with `docker logs -f`.

4. Expose ports
   * Add a tiny web server example.
   * Use `docker run -p 8080:80` to map a port from the host machine to the container.

5. Use environment variables
   * Show `docker run -e NAME=value`.
   * Explain how applications can read configuration from environment variables.

6. Add a `.dockerignore` file
   * Explain why the build context matters.
   * Exclude folders such as `.git`, `bin`, `obj`, `node_modules`, and temporary files.

7. Build a real application image
   * Replace the shell script with a small .NET, Node.js, Python, or Go application.
   * Show how application files are copied, restored, built, and executed.

8. Use multi-stage builds
   * Use one image for building the application and another smaller image for running it.
   * Explain how this reduces image size and avoids shipping build tools.

9. Publish an image to a registry
   * Tag an image for Docker Hub, GitHub Container Registry, or a private registry.
   * Show `docker push` and `docker pull`.

10. Run multiple containers with Docker Compose
   * Create a `docker-compose.yml` file.
   * Add an application container and a database container.
   * Show `docker compose up`, `docker compose down`, and where logs can be found.
