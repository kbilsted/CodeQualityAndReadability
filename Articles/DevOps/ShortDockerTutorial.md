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
   * [Clean up local Docker resources](#clean-up-local-docker-resources)
   * [Inspect containers and images](#inspect-containers-and-images)
   * [Run containers in the background](#run-containers-in-the-background)
   * [Expose ports](#expose-ports)
   * [Use environment variables](#use-environment-variables)


## Images and containers
In programming, we have classes and objects that are instances of classes. In the Docker world, an image is comparable to a class, and a container is comparable to an object.

### Creating our first image

Create a file named `Dockerfile` in an empty folder with this content:

```
FROM ubuntu:latest
```


### Build the image

In the same folder, run:

```
docker build -t my_stuff:1.0 -t my_stuff:latest .
```


Flags explained:

* `-t my_stuff:1.0` = a tag, meaning the name and version of your image
* `-t my_stuff:latest` = another tag for the same image
* `.` = build from the Dockerfile in the current folder

We tag the image so we can find it again easily. We use *two* tags, so we have both a versioned tag and a `latest` tag. The `latest` tag makes it easy to refer to the most recent build, similar to how we use `FROM ubuntu:latest`.


### See the result
Looking in the current folder will not show anything new:

```
> dir

Mode                 LastWriteTime         Length Name
----                 -------------         ------ ----
-a---          05-05-2026    08:16             37 Dockerfile
```

The image is only visible inside Docker's internal image store:

```
> docker image ls

REPOSITORY                 TAG       IMAGE ID       CREATED        SIZE
my_stuff                   1.0       4e759ff0d3fd   13 days ago    157MB
my_stuff                   latest    4e759ff0d3fd   13 days ago    157MB
...
```


### Save the image to the local filesystem
We can save the image to the filesystem. Docker saves images as tar files. Other formats, such as zip files, are not supported by `docker save`.


```
> docker save -o my.tar my_stuff:1.0
> dir

Mode                 LastWriteTime         Length Name
----                 -------------         ------ ----
-a---          05-05-2026    08:16             37 Dockerfile
-a---          05-05-2026    08:25       41.574.400 my.tar
```


## 'docker run' - Running the image as a container

To create a container from an image and get an interactive shell in Ubuntu Linux, run:

```
> docker run -it my_stuff:1.0

root@2bd442a1e2d1:/#
```

`-it` is important because Bash is interactive. Without it, Bash will stop immediately.

* `-i` keeps input open
* `-t` creates a terminal

We can also run the container with the command `ls` to see the content of the startup folder:

```
> docker run -t my_stuff:1.0 ls

bin  boot  dev  etc  home  lib  lib64  media  mnt  opt  proc  root  run  sbin  srv  sys  tmp  usr  var
```

## 'docker ps' - process status
After a container has terminated executing, it will still be around. We can see the process status using the  `ps` command.

```
> docker ps
CONTAINER ID   IMAGE     COMMAND   CREATED   STATUS    PORTS     NAMES
```

The list is empty. This is because there are no currently running containers. We can see all containers in the system using the `-a` flag.

```
> docker ps -a

CONTAINER ID   IMAGE             COMMAND      CREATED         STATUS                     PORTS     NAMES
3186acd26e02   my_stuff:1.0      "..."        6 seconds ago   Exited (0) 5 seconds ago             confident_diffie
``` 


## 'docker start' - restart an existing container
The command `docker run .. ` creates a new container. If you create a new container every time you want to run an image, you may end up with a lot of stopped containers. Another approach is to restart an existing container.

``` 
docker start confident_diffie
```

We use `confident_diffie`, but we could also have used the ID `3186acd26e02` as the parameter.




## 'ENTRYPOINT' and 'CMD' - command and argument on startup

`ENTRYPOINT` defines the main executable for the container when it is starting. `CMD` defines the argument that is passed along. 

Change the `Dockerfile` to:

```
FROM ubuntu:latest
ENTRYPOINT [ "echo" ]
CMD [ "Hello World", "again.." ]

> docker build -t my_stuff:latest .
> docker run my_stuff:latest

Hello World again..
```

If we pass a command parameter such as `ls`, as we did earlier, we do not get the folder content.

```
> docker run my_stuff:latest ls

ls
```

This happens because `ls` is passed as an argument to `echo`. The result is the same as running `echo ls`.

When debugging a faulty image, it can be helpful to alternate what is started. The `ENTRYPOINT` can be changed using  `--entrypoint`:

```
> docker run --entrypoint ls my_stuff:latest /

bin
boot
dev
...
```

Only one `CMD` instruction can be used. If the Dockerfile contains multiple `CMD` instructions, only the last one is used.



### When  'CMD' becomes the command

You may omit `ENTRYPOINT` and only use `CMD`. In this situation,
`CMD` defines the default command, that runs when the container starts. 

Let's modify our `Dockerfile` to:

```
FROM ubuntu:latest
CMD ["sh", "-c", "echo First && echo Second && echo Third"]
```

Build and run the new image:

```
> docker build -t my_stuff:3.0 -t my_stuff:latest .
> docker run my_stuff:latest

First
Second
Third
```

`CMD` can be overridden. We can replace the command at runtime:

```
> docker run -t my_stuff:latest ls

bin  boot  dev  etc  home  lib  lib64  media  mnt  opt  proc  root  run  sbin  srv  sys  tmp  usr  var
```

The `ls` argument replaces the whole `CMD` definition from the `Dockerfile`. So Docker runs `ls` instead of the default command.


### Warning about '[]'
The `[]` parentheses enable us to supply more than one argument to `ENTRYPOINT` and `CMD`. It also changes how the arguments are handled.

This is called the *exec form*. The syntax is a JSON array, so every value must use double quotes:

```
ENTRYPOINT ["echo"]
CMD ["Hello World"]
```

Docker does not run this through a shell. It runs `echo` directly and passes `Hello World` as an argument. This makes argument handling more predictable, especially when arguments contain spaces.

Because the syntax is JSON, escaping also follows JSON rules. If you need a quote inside an argument, escape it with `\"`. If you need a Windows path with backslashes, the backslashes must be escaped as `\\`.

For example:

```
CMD ["sh", "-c", "echo \"Hello World\""]
```

Without `[]`, Docker uses the *shell form*. The command is treated as a single string and is run through a shell. That can be useful for shell features such as `&&`, pipes, and variable expansion, but it also means Docker is no longer passing the arguments in the same direct way.

Try removing the `[]` and see how the Docker image no longer works as expected.

```
FROM ubuntu:latest
ENTRYPOINT "echo"
CMD "Hello World"
```
and run

```
> docker build -t my_stuff:latest .
 2 warnings found (use docker --debug to expand):
 - JSONArgsRecommended: JSON arguments recommended for ENTRYPOINT to prevent unintended behavior related to OS signals (line 2)
 - JSONArgsRecommended: JSON arguments recommended for CMD to prevent unintended behavior related to OS signals (line 3)

> docker run my_stuff:latest

(( missing output!))
```

In this example, `ENTRYPOINT "echo"` is shell form. The `CMD "Hello World"` value is not passed to `echo` the same way as in the JSON-array version, so `echo` runs without the text we expected.

Feel free to dig more into this if you find it interesting. For now, perhaps just remember to use `[]` unless you specifically need shell behavior.





## 'WORKDIR' - setting the current directory
It is normal to operate on a number of folders in the Docker image. Often we separate the `/app/` folder from the `/data/` folder.

To change the current folder, we use `WORKDIR path`. If the path does not exist, it is created.




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
WORKDIR /app
COPY print_numbers.sh /app/print_numbers.sh
RUN chmod +x /app/print_numbers.sh
ENTRYPOINT ["/bin/bash"]
CMD ["/app/print_numbers.sh"]
```

We copy our application to the `/app/` folder to separate it from the tools we may need to install later. We use `chmod` to make the script executable. Finally, we set it as the default command using `ENTRYPOINT` + `CMD`.

```
> docker build -t my_stuff:latest .
> docker run my_stuff:latest

1
2
...
```



## Persist data inside 
A container is running on the host operating system, while being isolated from it. This means a program inside a container cannot write to the "outside" file system unless given permission. Let us play with storing state locally inside the container and then move on to storing the same state outside.

Change the `Dockerfile` and notice we create the folder `/data/`.

```
FROM ubuntu:latest
WORKDIR "/data/"
CMD ["/bin/bash", "-c", "echo 'test data' >> /data/log.txt"]
```

and run

```
> docker build -t my_stuff:latest .
> docker run my_stuff:latest
```

Our code appends "test data"  to the file "log.txt". Let's run it a few times and inspect the content within the container.
If we just issue another `docker run` a new container is spun up. We want to re-execute the existing container, in order to see the effect of many log lines in our log file. 

```
> docker ps -a

CONTAINER ID   IMAGE             COMMAND
3186acd26e02   my_stuff:latest   ...

> docker start 3186acd26e02
> docker start 3186acd26e02
> docker start 3186acd26e02
```

Let's inspect our file. We can do this a number of ways. 

To show the filesystem changes inside the container, we use `docker diff`.

```
> docker diff 3186acd26e02

C /data
A /data/log.txt
```

To see the content we copy it out of the container

```
> docker cp 3186acd26e02:/data/log.txt ./locallog.txt
> cat ./locallog.txt

test data
test data
test data
test data
```

As expected we see the log lines, one for each time we started the container.




## `VOLUME` - Storing outside the container
Normally, files written inside a container disappear when the container is removed. 

Perhaps you want to run a database as a container. If we store the data rows inside the image, we cannot upgrade the DB using a new image without losing all our data.

A Docker volume is storage managed by Docker that can live longer than any single container. We define a `VOLUME` inside our `Dockerfile` and then when invoking the image, specify where on the host operating system that points to.

Let's modify our `Dockerfile`:

```
FROM ubuntu:latest
VOLUME /data
WORKDIR /data
CMD ["/bin/bash", "-c", "echo 'test data' >> /data/log.txt"]

```

* We must run the container using a `-v` flag or state is stored in an anonymous volume that may be hard to find
* We can see all volumes using `docker volume ls`

First try *omitting* the volume, and observe that an anonymous volume is created.

```
> docker build -t my_stuff:latest .
>
> docker volume ls
> docker run my_stuff:latest
> docker volume ls

DRIVER    VOLUME NAME
local     7c7044f1f1513b0aab2e6a62a35...
``` 

### `docker volume create` - target a volume file on the host machine

To create a volume we use `docker volume create myvol`

Let's see the whole sequence of executions

```
> docker volume create myvol
> docker run -v myvol:/data my_stuff:latest
> docker volume ls
DRIVER    VOLUME NAME
local     7c7044f1f1513b0aab2e6a62a35...
local     myvol
```

When we run the image again or start the container, both approaches will append to the volume. We use the `--rm` flag to remove the container after use

```
> docker run --rm -v myvol:/data my_stuff:latest
> docker start 1513b..
> docker run --rm -v myvol:/data my_stuff:latest cat /data/log.txt
test data
test data
test data
```

The file survived because it was written to the volume, not only to the container's own filesystem.

When you are done with the volume, remove it:

```
> docker volume rm myvol
```


## Binding Mount - target a folder on the host machine

Sometimes you do not want Docker to manage the storage. Instead, you want the container to read and write directly to a normal folder on your machine. This is called a bind mount.

Create a folder on the host machine and run the image. If you are on Windows, you must use `\` for the path. We use `.\host-data` to denote the sub-folder from where we run.

```
> mkdir host-data
> docker run --rm -v .\host-data:/data my_stuff:latest
```


The container writes the file to `/data/log.txt`, but `/data` points to the `host-data` folder on your machine. Let us now look inside the folder:


```
> dir host-data\
Mode                 LastWriteTime         Length Name
----                 -------------         ------ ----
-a---          05-05-2026    08:38             10 log.txt

> cat host-data/log.txt
test data
```

### Running a console program as a container
Instead of installing the formatting tool QUARTO on our host operating system, we can containerize it. This has the advantage
that the tool, should it be infected with mallware, can only reach the files I tell it to operate on. The idea is to have a docker image where quarto will be installed. When running the tool it will use a volume that we bind to our file system.

we make the image 

```
FROM ubuntu:latest

# we use only one run-to reduce the number of layers

ARG QUARTO_VERSION=1.9.37

RUN set -eux; \
    apt-get update; \
  # --no-install-recommends :: to reduce the download
  # ca-certificates ::  er god at have med, når du downloader via HTTPS.
    apt-get install -y --no-install-recommends wget ca-certificates; \
    wget -O /tmp/quarto.deb "https://github.com/quarto-dev/quarto-cli/releases/download/v${QUARTO_VERSION}/quarto-${QUARTO_VERSION}-linux-amd64.deb"; \
    apt-get install -y /tmp/quarto.deb; \
    rm /tmp/quarto.deb; \
    rm -rf /var/lib/apt/lists/*

WORKDIR /data

ENTRYPOINT [ "quarto" ]

VOLUME /data
```

build the image `docker build -t quarto:latest .`



`docker run -it --rm   --mount type=bind,source=C:\src\Build-Your-Own-Git\,target=/data quarto:latest render book`


* -it run interactively
* --rm remove the container after use
* --mount bind the /data folder to my local folder
* quarto:latest the image to run
* render book, pass `render` and the folder name `book` to quarto


to debug the image also use `--entrypoint /bin/bash` 



## Volume summary

*Volume mounts* are useful when Docker should own the storage. They are usually a good default for database files and other container-owned state.

*Bind mounts* are useful when the host machine and the container should share a specific folder. During software development, they make it transparent to see and change files.



## `RUN` - execute during build time

`RUN` executes commands at build time. This is different from `CMD`/`ENTRYPOINT` that execute when the container starts.

We use curl to fetch data and print to the screen.
We use `set -eux`:

* `-e`: stop the build if a command fails
* `-u`: fail on undefined variables
* `-x`: print commands as they run

The `Dockerfile` content. For security reasons ubuntu is not shipped with curl out of the box, so we need first to install it.
```
FROM ubuntu:latest
WORKDIR /data

RUN set -eux; \
	 echo "install curl"; \
    apt-get update; \
    apt-get install -y --no-install-recommends curl ca-certificates; \
    rm -rf /var/lib/apt/lists/*

RUN set -eux; \
	echo "fetching"; \
	curl -fsSL https://example.com -o /data/example.content.txt; 

RUN set -eux; \
	echo "file created"; \
	ls -l /data/example.content.txt; \
	head -n 5 /data/example.content.txt;
```
to see the content fetched from curl, we need to use the `--progress=plain` flag

```
> docker build --no-cache --progress=plain -t my_stuff:latest .

...
#7 [4/5] RUN set -eux;  echo "fetching";        curl -fsSL https://example.com -o /data/example.content.txt;
#7 0.361 fetching
#7 0.361 + echo fetching
#7 0.361 + curl -fsSL https://example.com -o /data/example.content.txt
#7 DONE 0.6s

#8 [5/5] RUN set -eux;  echo "file created";    ls -l /data/example.content.txt;        head -n 5 /data/example.content.txt;
#8 0.380 file created
#8 0.380 + echo file created
#8 0.380 + ls -l /data/example.content.txt
#8 0.385 -rw-r--r-- 1 root root 528 May 11 07:26 /data/example.content.txt
#8 0.386 + head -n 5 /data/example.content.txt
#8 0.389 <!doctype html><html lang="en"><head><title>Example Domain</title><meta name="viewport" content="width=device-width, initial-scale=1"><style>body{background:#eee;width:60vw;margin:15vh auto;font-family:system-ui,sans-serif}h1{font-size:1.5em}div{opacity:0.8}a:link,a:visited{color:#348}</style></head><body><div><h1>Example Domain</h1><p>This domain is for use in documentation examples without needing permission. Avoid use in operations.</p><p><a href="https://iana.org/domains/example">Learn more</a></p></div></body></html>
#8 DONE 0.4s
...
```



## `docker inspect`  - looking inside 


Docker can show a lot of metadata about both images and containers. The command is `docker inspect`.

To inspect an image

```
> docker image ls
> docker inspect my-stuff:latest
[
  {
    "Config": {
            "ArgsEscaped": true,
            "AttachStderr": false,
            "AttachStdin": false,
            "AttachStdout": false,
            "Cmd": [
                "/bin/bash",
                "-c",
                "echo 'test data' >> /data/log.txt"
            ],
...
```

The full output is long. In practice, it is often better to ask for one value.
First run an image that does not exit immediately. 

```
> docker run -it --rm my-stuff:latest sh
```

in a different terminal issue

```
> docker ps   
ONTAINER ID   IMAGE             COMMAND   CREATED       
3aa1295a1a3c   my-stuff:latest   "sh"      7 seconds ago   
> docker inspect 3aa1295a1a3c --format "{{.State.Status}}"
running
Show the image used by the container:

```
> docker inspect inspect-demo --format "{{.Config.Image}}"

my_stuff:latest
```

Show environment variables configured in the container:

```
> docker inspect inspect-demo --format "{{json .Config.Env}}"

["PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin","APP_MODE=demo"]
```


## `docker logs` - show what is printed to stdout

Logs are separate from inspect. `docker inspect` shows configuration and metadata. `docker logs` shows what the container printed to stdout and stderr.

```
> docker logs inspect-demo

app mode is demo
```

Clean up the container when done:

```
> docker rm inspect-demo

inspect-demo
```


## Run containers in the background

So far, most examples have run in the foreground. This means the terminal waits until the container stops. Servers and long-running processes are usually started in the background.

Change the `Dockerfile` to:

```
FROM ubuntu:latest
CMD ["sh", "-c", "i=1; while true; do echo tick $i; i=$((i+1)); sleep 2; done"]
```

Build the image:

```
> docker build -t my_stuff:latest .
```

Run it in detached mode using `-d`:

```
> docker run -d --name background-demo my_stuff:latest

7c0d90b30c8d8f26d1f5b64c0c0a6a2d8a6b8bb8d2f50ddf0d63e12b3f1d45d9
```

Docker prints the container ID and returns control to your terminal.

See that it is running:

```
> docker ps

CONTAINER ID   IMAGE             COMMAND                  STATUS         NAMES
7c0d90b30c8d   my_stuff:latest   "sh -c 'i=1; while..."   Up 6 seconds   background-demo
```

Show the logs:

```
> docker logs background-demo

tick 1
tick 2
tick 3
```

Follow the logs live:

```
> docker logs -f background-demo

tick 1
tick 2
tick 3
tick 4
```

Press `Ctrl+C` to stop following the logs. This stops the log view, not the container.

Stop the container:

```
> docker stop background-demo

background-demo
```

Remove it:

```
> docker rm background-demo

background-demo
```

Detached mode is useful for web servers, databases, message queues, and anything else that should keep running while you use the terminal for other commands.


## Expose ports

A container has its own network namespace. If a process listens on a port inside the container, that port is not automatically available on your host machine.

We need two things:

* The application must listen on a port inside the container
* `docker run -p` must map a host port to the container port

Change the `Dockerfile` to a tiny web server:

```
FROM python:3.12-alpine
WORKDIR /site
RUN echo "<h1>Hello from Docker</h1>" > index.html
EXPOSE 8000
CMD ["python", "-m", "http.server", "8000", "--bind", "0.0.0.0"]
```

Build it:

```
> docker build -t my_stuff:latest .
```

Run it in the background and map host port `8080` to container port `8000`:

```
> docker run -d --name web-demo -p 8080:8000 my_stuff:latest

9b4ab9f5d573f6a4a5c6d7c8a9b0c1d2e3f4567890abcdef1234567890abcdef
```

The syntax is:

```
-p HOST_PORT:CONTAINER_PORT
```

So this:

```
-p 8080:8000
```

means:

* Use port `8080` on your machine
* Forward traffic to port `8000` inside the container

Try it from PowerShell:

```
> Invoke-WebRequest http://localhost:8080

StatusCode        : 200
StatusDescription : OK
Content           : <h1>Hello from Docker</h1>
```

Or open this URL in a browser:

```
http://localhost:8080
```

See the port mapping:

```
> docker ps

CONTAINER ID   IMAGE             PORTS                    NAMES
9b4ab9f5d573   my_stuff:latest   0.0.0.0:8080->8000/tcp   web-demo
```

`EXPOSE 8000` documents that the container listens on port `8000`. It does not publish the port to the host machine by itself. The `-p` flag is what makes the port reachable from your machine.

Stop and remove the container:

```
> docker stop web-demo
> docker rm web-demo
```

You can run the same image on another host port:

```
> docker run --rm -p 5000:8000 my_stuff:latest
```

Now the same container port is available on:

```
http://localhost:5000
```


## Use environment variables

Environment variables are a common way to configure containers. The image contains the application. The environment variables decide how it should behave in this specific run.

Change the `Dockerfile` to:

```
FROM ubuntu:latest
ENV MESSAGE="Hello from the Dockerfile"
ENV REPEAT=1
CMD ["sh", "-c", "i=1; while [ $i -le $REPEAT ]; do echo \"$MESSAGE\"; i=$((i+1)); done"]
```

Build and run:

```
> docker build -t my_stuff:latest .
> docker run --rm my_stuff:latest

Hello from the Dockerfile
```

Override `MESSAGE` at runtime:

```
> docker run --rm -e MESSAGE="Hello from docker run" my_stuff:latest

Hello from docker run
```

Set more than one environment variable:

```
> docker run --rm -e MESSAGE="Configured at runtime" -e REPEAT=3 my_stuff:latest

Configured at runtime
Configured at runtime
Configured at runtime
```

You can inspect the environment variables on a container. First run it without `--rm`, so the stopped container remains available:

```
> docker run --name env-demo -e MESSAGE="Inspect me" my_stuff:latest

Inspect me
```

Then inspect it:

```
> docker inspect env-demo --format "{{json .Config.Env}}"

["PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin","MESSAGE=Inspect me","REPEAT=1"]
```

Clean up:

```
> docker rm env-demo

env-demo
```

Use environment variables for configuration such as:

* application mode
* connection strings
* feature flags
* log level
* port numbers

Do not put passwords or tokens directly in a `Dockerfile`. Values in a `Dockerfile` become part of the image history. For local experiments `-e` is fine, but real systems usually use Docker secrets, Kubernetes secrets, or the secret store from the platform running the container.










## Clean up local Docker resources

When experimenting with Docker, you quickly create containers, images, volumes, and networks. This is normal, but it is useful to know how to clean up after yourself.

Let us start by creating a container we can remove.

Change the `Dockerfile` to:

```
FROM ubuntu:latest
CMD ["sh", "-c", "echo cleanup demo"]
```

Build and run it:

```
> docker build -t my_stuff:latest .
> docker run --name cleanup-demo my_stuff:latest

cleanup demo
```

The container has now stopped, but it still exists.

```
> docker ps

CONTAINER ID   IMAGE     COMMAND   CREATED   STATUS    PORTS     NAMES

> docker ps -a

CONTAINER ID   IMAGE             COMMAND                  CREATED          STATUS                      PORTS     NAMES
2f4c0f7f3f41   my_stuff:latest   "sh -c 'echo clean..."   10 seconds ago   Exited (0) 8 seconds ago              cleanup-demo
```

`docker ps` shows running containers. `docker ps -a` shows all containers, including stopped containers.

Remove the stopped container:

```
> docker rm cleanup-demo

cleanup-demo
```

If the container is running, you must stop it before removing it:

```
> docker stop cleanup-demo
cleanup-demo

> docker rm cleanup-demo
cleanup-demo
```

Images are different from containers. A container is created from an image. Removing a container does not remove the image.

```
> docker image ls my_stuff

REPOSITORY   TAG       IMAGE ID       CREATED          SIZE
my_stuff     latest    4e759ff0d3fd   2 minutes ago    78.1MB
```

During small experiments it is often useful to use `--rm`. This removes the container automatically when it stops:

```
> docker run --rm my_stuff:latest

cleanup demo
```

There is no stopped container left behind:

```
> docker ps -a --filter name=cleanup-demo

CONTAINER ID   IMAGE     COMMAND   CREATED   STATUS    PORTS     NAMES
```

When you are done with the image, remove it:

```
> docker image rm my_stuff:latest

Untagged: my_stuff:latest
Deleted: sha256:4e759ff0d3fd...
```

If Docker says the image is used by a container, remove the container first.

Docker also has prune commands. They are useful, but read the output before accepting.

```
> docker container prune

WARNING! This will remove all stopped containers.
Are you sure you want to continue? [y/N]
```

Other cleanup commands are:

```
> docker image prune
> docker volume prune
> docker system prune
```

Be extra careful with `docker volume prune`. Volumes often contain data, for example database files.


















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
