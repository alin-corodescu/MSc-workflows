# Workflows

## Goals
* Performance analysis at a granular level of **container** based workflow orchestration system for **heterogenous** hardware. Understand the **implications** of the DSL usage in the functionality and performance of workflows
* Overcoming the identified issues by designing an **extensible** workflow orchestration system that allows for **optimizations**. Identify the **required concepts** that need to be accessible outside of individual layers. Create a **framework of interfaces** designed to standardize the communication across layers and with the control plane.
* Framework focus should be on extensibility, modularity, providing a **plug-and-play** feel to the system. More accent on the interface definition rather than the implementation. Need to be easy to use to lower the entry bar to the world of big data workflows.
* Create a simulation environment for **heterogenous** hardware and different conditions to be able to identify potential issues before the deployment.

## Concepts handled by the WF orchestrator (control plane)
1. **Optimizations** through the inferrence of step characteristics + available hardware + DSL concepts passed down
   1.  **Data locality** = routing layer: communication between the Hardware -> DSL and WF orchestrator
   2.  **Step definition hints** for the WF orchestrator to better optimize the runtime aspects (when to scale, how to scale)
   Or better describe the characteristics of the step to be surfaced all the way to the DSL in a user friendly way
   and/or provide hints

       For example: a step is an aggregator => it will reduce the amount of data trasmitted, might make sense to put it closer to the data source?
2. **scalability and elasticity**
      step instances (dynamic, or manual + hints)
      control plane instances (message queue)
3. **Fault tolerance and recovery**
4. **Monitoring:**
    1. Simulation and profiling
    2. Live running monitoring and insights
5. **Enforcment** of workflow **constraints** (defined in the DSL, if any)

## Other ideas
* Data splitting:
  * Interfaces which allow for simple map reduce steps (or instructions on how to scale input and re-combine output correctly) 
      * Information allowing these kind of steps to be 
      Interface to surface this kind of information to the DSL (if desirable).

  * Not really belonging to the DSL, maybe it can be hidden away or  surfaced != it's own step (in general applicable for some standard operators)

* DSL <-> hardware:
    * Pool of available resources (for example, the cloud is nearly infinite, edge computing is cheap but unreliable, etc), interface that allows modelling this interaction
    Hardware layout, draw a line between different boundaries (edge, zones, wihtin machine, etc).

* Streaming results instead of batching them in between steps
* PassThrough (not writing intermediate results to the disk) - Network calls between containers on the same machine, are those streamed through memory only - think I read about something similar for Linux)
* Change communication mediums between different steps (such as step 1 and step 2 need to have encrypted channels in between them, web service might be better fitted for some steps) etc



## Todo
1. Experiment with the existing solution to get a feel of what can be done
1. Read papers on the Simulation
1. Gather diverse data sets, simulate/ run under different conditions
1. Decide on scope = as much as we can


Best things I can target:
    Data workflows as a service
    Focus on the step definitions and providing the required scaffolding to make writing steps easily.


How do I help then:
    1. Ease the design process by introducing a DSL allowing to specify Cloud/Edge differences
    2. Ease the step writing process by creating the necessary interfaces
    3. Do this without sacrificing (or acking) the limitations of using container technologies as opposed to native framework implementations for big data workflows
       1. Data Movement
       2. Need to flush data to the disk
       3. 